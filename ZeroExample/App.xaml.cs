using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using ViewModels;

namespace ZeroExample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        override protected void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo.CurrentCulture = new CultureInfo("cs-CZ");
            CultureInfo.CurrentUICulture = new CultureInfo("cs-CZ");

            var vm = new MainViewModel();

            // Initialize ReactiveUI with RxAppBuilder
            var app = RxAppBuilder.CreateReactiveUIBuilder()
                .WithWpf()
                .WithViewsFromAssembly(Assembly.GetExecutingAssembly())
                .WithRegistration(locator =>
                {
                    // Register your services here
                    locator.RegisterLazySingleton<IScreen>(() => vm);
                })
                .BuildApp();
        }

    }

}
