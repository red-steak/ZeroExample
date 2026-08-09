
using Microsoft.Win32;
using Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using ViewModels.Helpers;
using ViewModels.Helpers.Classes;

namespace ViewModels
{
    public partial class MainViewModel: ReactiveObject, IScreen
    {
        static string _defaultTitle = " ❤ -> {0}";
        static Guid _dialogGuid = new("D4E3F2A1-B5C6-4D7E-8F9A-0B1C2D3E4F5A");

        #region REACTIVE PROPERTIES

        [Reactive] string _title = _defaultTitle;
        [Reactive] string _statusBar = _defaultTitle;
        [Reactive] FileVM? _file;
        [Reactive] ValidationVM _val = new();
        [Reactive] ObservableCollection<CarVM> _cars = [];
        [Reactive] int _totalCars;
        [Reactive] int _selectedTabIndex = 0;
        [Reactive] int _progressBarValue = 0;
        [Reactive] int _progressBarMax = 99;

        // Weekend sums
        [Reactive] AggHelper<WeekendSumsGroupByModel> _weekendSums = new();
        // Non-weekend sums
        [Reactive] AggHelper<NonWeekendSumsGroupByModel> _nonWeekendSums = new();
        // Sums - totals!
        [Reactive] AggHelper<SumsGroupByModel> _sums = new();

        // more additional sums for VAT,
        // Easter etc...
        [Reactive] AggHelper<SumsGroupByVat> _vatSums = new();
        [Reactive] AggHelper<SumsGroupByModelSoldDuringEaster> _sumsSoldDuringEaster = new();
        [Reactive] AggHelper<SumsGroupByDayOfWeek> _sumsDayOfWeek = new();
        [Reactive] AggHelper<SumsGroupByYear> _sumsYear = new();

        #endregion REACTIVE PROPERTIES

        #region REACTIVE COMMANDS

        [ReactiveCommand]
        public void OpenFile()
        {
            Open();
        }

        [ReactiveCommand]
        public async Task Save()
        {
            await SaveData();
        }

        [ReactiveCommand]
        public async Task Generate()
        {
            await GenerateData();
        }

        [ReactiveCommand]
        public void GenerateWithNulls()
        {
            GenerateDataWithNulls();
        }

        #endregion REACTIVE COMMANDS

        #region PRIVATE METHODS

        void Open()
        {
            OpenFileDialog dialog = new()
            {
                ForcePreviewPane = true,
                AddToRecent = true,
                CheckPathExists = true,
                CheckFileExists = true,
                ClientGuid = _dialogGuid,
                DefaultDirectory = $@"{Application.Current.StartupUri}\Data",
                Filter = "xml file|*.xml",
                Multiselect = false,
                Title = "Select an XML file"
            };

            var result = dialog.ShowDialog(Application.Current.MainWindow);
            
            if (result == true)
            {
                string filePath = dialog.FileName;

                if (System.IO.File.Exists(filePath) && !string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        string fileContent = System.IO.File.ReadAllText(filePath);
                        var fileVM = new FileVM($"{Path.GetDirectoryName(filePath!)}", Path.GetFileName(filePath), fileContent);
                        File = fileVM;
                        Cars.Clear();
                        ClearTotals();

                        Val.Name = "Base validation if the file is XML...";
                        if (ValidateFileContent(fileVM.DefaultFileContent))
                        {
                            if (!LoadXML(fileVM.DefaultFileContent, out var _errorMessage))
                            {
                                Val.Name = "XML parsing failed!";
                                Val.Result = ValidationResult.Invalid;
                                Val.ResultDescrition = _defaultTitle.Replace("{0}", $"Error: {_errorMessage}");
                                StatusBar = _defaultTitle.Replace("{0}", $"Error: {_errorMessage}");
                                return;
                            }
                            else
                            {
                                Val.Name = "XML parsing succeeded.";
                                Val.Result = ValidationResult.Valid;
                            }
                        }
                    }
                    catch (UnauthorizedAccessException unauthAccessEx)
                    {
                        StatusBar = _defaultTitle.Replace("{0}", $"Error: Unauthorized access to file: {filePath}. {unauthAccessEx.Message}");
                        return;
                    }
                    catch (IOException ioEx)
                    {
                        StatusBar = _defaultTitle.Replace("{0}", $"Error: IO exception while reading file: {filePath}. {ioEx.Message}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        StatusBar = _defaultTitle.Replace("{0}", $"Error: An unexpected error occurred while opening file: {filePath}. {ex.Message}");
                        return;
                    }
                }

                Title = _defaultTitle.Replace("{0}", $"Opened: {filePath}");
                StatusBar = _defaultTitle.Replace("{0}", $"Opened: {filePath}");
            }
        }

        void ClearTotals()
        {
            Sums?.Totals = [];
            WeekendSums?.Totals = [];
            NonWeekendSums?.Totals = [];
            SumsDayOfWeek?.Totals = [];
            SumsSoldDuringEaster?.Totals = [];
            SumsYear?.Totals = [];            
        }

        bool ValidateFileContent(string fileContent)
        {
            var validationVM = Val;
            
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                validationVM.Result = ValidationResult.Invalid;
                validationVM.ResultDescrition = "File content is empty.";
                return false;
            }
            else if (!fileContent.StartsWith("<"))
            {
                validationVM.Result = ValidationResult.Invalid;
                validationVM.ResultDescrition = "File content does not appear to be valid XML.";
                return false;
            }
            else if (!fileContent.EndsWith(">"))
            {
                validationVM.Result = ValidationResult.Invalid;
                validationVM.ResultDescrition = "File content does not appear to be valid XML.";
                return false;
            }
            else if (!fileContent.Contains("</"))
            {
                validationVM.Result = ValidationResult.Invalid;
                validationVM.ResultDescrition = "File content does not appear to be valid XML.";
                return false;
            }
            else if (!fileContent.Contains("<"))
            {
                validationVM.Result = ValidationResult.Invalid;
                validationVM.ResultDescrition = "File content does not appear to be valid XML.";
                return false;
            }
            else
            {
                validationVM.Result = ValidationResult.Valid;
                validationVM.ResultDescrition = "File content is valid.";
            }

            return true;
        }

        bool LoadXML(string fileContent, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                XDocument doc = XDocument.Parse(fileContent, LoadOptions.PreserveWhitespace);
                var root = doc.Root;
                if (root is null || !root.Name.LocalName.ToUpperInvariant().Equals("CARS"))
                {
                    errorMessage = "Root element is not 'CARS'.";
                    return false;
                }

                var cars = root.Elements("Car");
                foreach (var car in cars)
                {
                    try
                    {
                        var model = car.Element("Model")?.Value;
                        var saleDate = car.Element("SaleDate")?.Value;
                        var price = car.Element("Price")?.Value;
                        var vat = car.Element("VAT")?.Value;
                        var carVM = new CarVM
                        {
                            Model = model,
                            SaleDate = DateTime.TryParse(saleDate, out var parsedDate) ? parsedDate : (DateTime?)null,
                            Price = double.TryParse(price,
                                NumberStyles.Any,
                                CultureInfo.GetCultureInfo("en-US"),
                                out var parsedPrice) ? parsedPrice : (double?)null,
                            Vat = double.TryParse(vat,
                                NumberStyles.Any,
                                CultureInfo.GetCultureInfo("en-US"),
                                out var parsedVAT) ? parsedVAT : (double?)null
                        };
                        _cars.Add(carVM);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Error parsing car element: {ex.Message}";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            CalculateTotals();

            return true;
        }

        private void CalculateTotals()
        {
            // Weekend sums
            WeekendSums.Calculate([.. Cars]);

            // Non-weekend sums
            NonWeekendSums.Calculate([.. Cars]);
            // Sums
            Sums.Calculate([.. Cars]);

            // VAT sums
            VatSums.Calculate([.. Cars]);
            // Easter group by model sums
            SumsSoldDuringEaster.Calculate([.. Cars]);
            // Day of week sums
            SumsDayOfWeek.Calculate([.. Cars]);
            // Year sums
            SumsYear.Calculate([.. Cars]);
        }

        async Task GenerateData()
        {
            Cars.Clear();

            StatusBar = "Generating data ... ";
            ProgressBarValue = 0;
            ProgressBarMax = 10000 - 1;
            await Task.Run(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    var carVM = new CarVM
                    {
                        Model = $"SKODA N__{(i % 7) * (1 + Random.Shared.Next(7)) + Random.Shared.Next(7)}",
                        SaleDate = DateTime.Now.AddDays(-1 * (Random.Shared.Next(4) * Random.Shared.Next(13) * 30 + Random.Shared.Next(357))),
                        Price = 1000000 - (Random.Shared.Next(10) * 80000),
                        Vat = 20 - (Random.Shared.Next(5) * 2)
                    };

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Cars.Add(carVM);
                    }));
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CalculateTotals();
                    ProgressBarValue += 1;
                }));
            });

            StatusBar = _defaultTitle.Replace("{0}", $"Generated 10.000 random entries...");
            Val.Name = "Generated 10.000 random entries...";
            Val.Description = null;
            Val.Result = ValidationResult.Valid;
            Val.ResultDescrition = "Success!";
            ProgressBarValue = 0;
        }
        void SaveCars(List<Car> cars, string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("Cars"));

            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, cars);
        }

        private async Task SaveData()
        {
            StatusBar = "Saving data ... ";
            ProgressBarValue = 0;
            ProgressBarMax = Cars.Count - 1;

            SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "xml",
                AddToRecent = true,
                CheckPathExists = true,
                DefaultDirectory = $@"{Application.Current.StartupUri}\Data",
                Filter = "xml file|*.xml",
                Title = "Select or enter an XML file (name)"
            };

            var ok = dialog.ShowDialog();

            if (!ok.HasValue || !ok.Value) return;

            List<Car> cars = [.. Cars.Select(c => new Car() { Model = c.Model, Price = c.Price, SaleDate = c.SaleDate, Vat = c.Vat })];

            try
            {
                await Task.Run(() =>
                {
                    SaveCars(cars, dialog.FileName);
                });

                StatusBar = _defaultTitle.Replace("{0}", $"Saved {Cars.Count} entries...");
            }
            catch (Exception ex)
            {
                StatusBar = _defaultTitle.Replace("{0}", $"Error saving data: {ex.Message}");
            }

            ProgressBarValue = 0;
        }

        private void GenerateDataWithNulls()
        {
            Cars.Clear();
            for (int i = 0; i < 10000; i++)
            {
                var carVM = new CarVM
                {
                    Model = Random.Shared.Next(100) == 1 ? null : $"SKODA Model {i % 10}",
                    SaleDate = Random.Shared.Next(100) == 1 ? null : DateTime.Now.AddDays(-i % 30),
                    Price = Random.Shared.Next(80) == 1 ? null : 1000000 - (Random.Shared.Next(10) * 80000),
                    Vat = Random.Shared.Next(80) == 1 ? null : 20 - (Random.Shared.Next(5) * 2)
                };
                Cars.Add(carVM);
            }

            CalculateTotals();

            StatusBar = _defaultTitle.Replace("{0}", $"Generated 10.000 random entries with nulls...");
            Val.Name = "Generated 10.000 random entries with nulls...";
            Val.Description = null;
            Val.Result = ValidationResult.Invalid;
            Val.ResultDescrition = "There are nulls generated...";
        }

        #endregion PRIVATE METHODS

        #region ctor

        public MainViewModel()
        {
            Title = _defaultTitle.Replace("{0}", "Please open an XML file...");
            StatusBar = _defaultTitle.Replace("{0}", "Please open an XML file...");
        
            Cars.CollectionChanged += (s, e) =>
            {
                TotalCars = Cars.Count;
            };
        }

        #endregion ctor

        public RoutingState Router => throw new NotImplementedException();
    }

}
