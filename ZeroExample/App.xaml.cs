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
                    locator.RegisterLazySingleton<IReadService>(() => new ReadFileService());
                    locator.RegisterLazySingleton<ILoadService>(() => new LoadService());

                    locator.RegisterLazySingleton<MainViewModel>(() => new MainViewModel(Locator.Current.GetService<IReadService>()!, Locator.Current.GetService<ILoadService>()!));
                    locator.RegisterLazySingleton<MainWindow>(() => new MainWindow(Locator.Current.GetService<MainViewModel>()!));
                })
                .BuildApp();

            var mainWindow = Locator.Current.GetService<MainWindow>();
            mainWindow!.Show();
        }
    }

}
