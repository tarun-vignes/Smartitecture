using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smartitecture.Core.DependencyInjection;
using Smartitecture.WPF.ViewModels;
using Smartitecture.WPF.Views;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Smartitecture.WPF
{
    public partial class App : Application
    {
        private IHost _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Create host with dependency injection
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    // Add core services
                    services.AddSmartitectureCore();
                    
                    // Add WPF services
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            // Start the host
            await _host.StartAsync();

            // Create and show main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }
    }

    // Value converter for inverting boolean values
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }
}
