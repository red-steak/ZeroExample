
using Interfaces;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using ViewModels.Helpers;
using ViewModels.Helpers.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ViewModels
{
    public partial class MainViewModel: ReactiveObject, IScreen
    {
        static string _defaultTitle = " ❤ -> {0}";
        static Guid _dialogGuid = new("D4E3F2A1-B5C6-4D7E-8F9A-0B1C2D3E4F5A");

        private IReadService _readService;
        private ILoadService _loadService;

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
        public async Task OpenFile()
        {
            CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = false,
                DefaultExtension = "xml",                
                DefaultDirectory = $@"{Application.Current.StartupUri}\Data",
                Multiselect = false,
                Title = "Select an XML file"
            };

            var result = dialog.ShowDialog(Application.Current.MainWindow);

            if (result == CommonFileDialogResult.Ok)
            {
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ClearTotals();
                })); 
                
                try
                {
                    await Open(dialog.FileName);
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CalculateTotals();
                    }));

                }
                catch (Exception ex)
                { 
                    LogToApp("Opening a XML file. An error occurred!", ex.Message);
                }
            }
        }

        [ReactiveCommand]
        public void Save()
        {
            SaveData();
        }

        [ReactiveCommand]
        public async Task Generate()
        {
            await GenerateData(100000);
        }

        [ReactiveCommand]
        public async Task GenerateWithNulls()
        {
            await GenerateData(100000, true);
        }

        #endregion REACTIVE COMMANDS

        #region PRIVATE METHODS

        async Task Open(string fileName)
        {
            string filePath = fileName;

            var serviceResult = await _readService.ReadAsync(filePath);

            if (!serviceResult.IsSuccess)
            {
                LogToApp("File reading failed!", serviceResult.ErrorMessage);
                return;
            }
            else
            {
                var parsingResult = await _loadService.LoadAsync(serviceResult.Response);

                if (!parsingResult.Result)
                {
                    LogToApp("XML parsing failed!", parsingResult.ErrorMessage!);
                    return;
                }
                else
                {
                    var fileVM = new FileVM($"{Path.GetDirectoryName(filePath!)}", Path.GetFileName(filePath));
                    File = fileVM;
                    LogToApp("File reading and parsing succeeded!", string.Empty, true);
                    Val.Result = ValidationResult.Valid;
                }


                Val.Name = "Base validation if the file is XML...";
                if (!LoadXML(parsingResult.Document, out var _errorMessage))
                {
                    LogToApp("XML loading failed!", _errorMessage);
                }
                else
                {
                    Val.Name = "XML parsing succeeded.";
                    Val.Result = ValidationResult.Valid;
                }

                Title = _defaultTitle.Replace("{0}", $"Opened: {filePath}");
                StatusBar = _defaultTitle.Replace("{0}", $"Opened: {filePath}");
            }
        }

        private void LogToApp(string context, string errorMessage, bool isValid = false)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Val.Name = context;
                Val.Result = isValid ? ValidationResult.Valid : ValidationResult.Invalid;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    Val.ResultDescrition = isValid ? "Success!" : "Error!";
                    StatusBar = _defaultTitle.Replace("{0}", isValid ? "Success!" : "Error!");
                }
                else
                {
                    Val.ResultDescrition = _defaultTitle.Replace("{0}", $"Error: {errorMessage}");
                    StatusBar = _defaultTitle.Replace("{0}", $"Error: {errorMessage}");
                }
            }));
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

        bool LoadXML(XDocument? document, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Cars.Clear();
                    ClearTotals();
                }));

                if (document is null)
                {
                    errorMessage = "XML Document doesn't exists!";
                    return false;
                }

                var root = document?.Root;
                if (root is null)
                {
                    errorMessage = "Root element doesn't exists!";
                    return false;
                }

                var cars = root.Elements();
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

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _cars.Add(carVM);
                        }));
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Error parsing xml element: {ex.Message}";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

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

        async Task GenerateData(int count, bool withNulls = false)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Cars.Clear();
                StatusBar = "Generating data ... ";
                ProgressBarValue = 0;
                ProgressBarMax = count - 1;
            }));

            for (int i = 0; i < count; i++)
            {
                string? model = string.Empty;
                DateTime? saleDate = DateTime.MinValue;
                double? price = double.MinValue;
                double? vat = double.MinValue;

                if (withNulls)
                {
                    model = Random.Shared.Next(100) == 1 ? null : $"SKODA N__{(i % 7) * (1 + Random.Shared.Next(7)) + Random.Shared.Next(7)}";
                    saleDate = Random.Shared.Next(100) == 1 ? null : DateTime.Now.AddDays(-1 * (Random.Shared.Next(4) * Random.Shared.Next(13) * 30 + Random.Shared.Next(357)));
                    price = Random.Shared.Next(100) == 1 ? null : 1000000 - (Random.Shared.Next(10) * 80000);
                    vat = Random.Shared.Next(100) == 1 ? null : 22 - (Random.Shared.Next(5) * 2);
                }
                else
                {
                    model = $"SKODA N__{(i % 7) * (1 + Random.Shared.Next(7)) + Random.Shared.Next(7)}";
                    saleDate = DateTime.Now.AddDays(-1 * (Random.Shared.Next(4) * Random.Shared.Next(13) * 30 + Random.Shared.Next(357)));
                    price = 1000000 - (Random.Shared.Next(10) * 80000);
                    vat = 22 - (Random.Shared.Next(5) * 2);
                }

                var carVM = new CarVM
                {
                    Model = model,
                    SaleDate = saleDate,
                    Price = price,
                    Vat = vat
                };

                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Cars.Add(carVM);
                }));
            }

            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                CalculateTotals();
                ProgressBarValue += 1;
            }));

            var mess = withNulls ? $"Generated {count} random entries with nulls..." : $"Generated {count} random entries...";

            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusBar = _defaultTitle.Replace("{0}", mess);
                Val.Name = mess;
                Val.Description = null;
                Val.Result = ValidationResult.Valid;
                Val.ResultDescrition = "Success!";
                ProgressBarValue = 0;
            }));
        }

        void SaveCars(List<Car> cars, string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("Cars"));

            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, cars);
        }

        private void SaveData()
        {
            StatusBar = "Saving data ... ";
            ProgressBarValue = 0;
            ProgressBarMax = Cars.Count - 1;

            CommonSaveFileDialog dialog = new()
            {
                DefaultExtension = "xml",
                AlwaysAppendDefaultExtension = true,
                DefaultFileName = "Cars.xml",
                DefaultDirectory = $@"{Application.Current.StartupUri}\Data",
                Title = "Select or enter an XML file (name)"
            };

            CommonFileDialogResult ok = dialog.ShowDialog();
            if (!(ok == CommonFileDialogResult.Ok)) return;

            List<Car> cars = [.. Cars.Select(c => new Car() { Model = c.Model, Price = c.Price, SaleDate = c.SaleDate, Vat = c.Vat })];

            try
            {
                var fn = dialog.FileName;
                var fileName = fn.EndsWith(".xml") ? fn : $"{fn}.xml"; 
                SaveCars(cars, fileName);
                StatusBar = _defaultTitle.Replace("{0}", $"Saved {Cars.Count} entries...");
            }
            catch (Exception ex)
            {
                StatusBar = _defaultTitle.Replace("{0}", $"Error saving data: {ex.Message}");
            }

            ProgressBarValue = 0;
        }

        #endregion PRIVATE METHODS

        #region ctor

        public MainViewModel(IReadService readService, ILoadService loadService)
        {
            Title = _defaultTitle.Replace("{0}", "Please open an XML file...");
            StatusBar = _defaultTitle.Replace("{0}", "Please open an XML file...");

            Cars.CollectionChanged += (s, e) =>
            {
                TotalCars = Cars.Count;
            };

            _readService = readService;
            _loadService = loadService;
        }

        #endregion ctor

        public RoutingState Router => throw new NotImplementedException();
    }

}
