using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smartitecture.Core.DependencyInjection;
using Smartitecture.Core.Options;
using Smartitecture.WPF.ViewModels;
using Smartitecture.WPF.Views;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using Serilog;
using Serilog.Events;

namespace Smartitecture.WPF
{
    public partial class App : Application
    {
        private IHost _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Configure Serilog for better logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/smartitecture-.log", 
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            try
            {
                Log.Information("Starting Smartitecture application...");

                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                // Create host with dependency injection
                _host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.Sources.Clear();
                        config.AddConfiguration(configuration);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // Configure options
                        services.Configure<PythonApiOptions>(
                            context.Configuration.GetSection(PythonApiOptions.SectionName));
                        
                        // Add core services (without hosted service for WPF)
                        services.AddSmartitectureCoreForWpf(context.Configuration);
                        
                        // Add WPF services
                        services.AddSingleton<MainViewModel>();
                        services.AddSingleton<MainWindow>();
                    })
                    .UseSerilog()
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSerilog(dispose: true);
                        logging.SetMinimumLevel(LogLevel.Trace);
                    })
                    .Build();

                Log.Information("Host built successfully");

                // Start the host
                await _host.StartAsync();
                Log.Information("Host started successfully");

                // Create and show main window
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

                Log.Information("Application started successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }

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
