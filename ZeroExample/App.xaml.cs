using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Builder;
using Services;
using Splat;
using System.Globalization;
using System.Reflection;
using System.Windows;
using ViewModels;

namespace ZeroExample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo.CurrentCulture = new CultureInfo("cs-CZ");
            CultureInfo.CurrentUICulture = new CultureInfo("cs-CZ");


            var app = RxAppBuilder.CreateReactiveUIBuilder()
                .WithWpf()
                .WithViewsFromAssembly(Assembly.GetExecutingAssembly())
                .WithRegistration(locator =>
                {
                    var readService = new ReadFileService();
                    var loadService = new LoadService();

                    locator.RegisterLazySingleton<IReadService>(() => readService);
                    locator.RegisterLazySingleton<ILoadService>(() => loadService);

                    var vm = new MainViewModel(readService, loadService);

                    locator.RegisterLazySingleton(() => vm);
                    locator.RegisterLazySingleton(() => new MainWindow(vm));
                })
                .BuildApp();

            var mainWindow = Locator.Current.GetService<MainWindow>();
            mainWindow!.Show();
        }
    }

}
