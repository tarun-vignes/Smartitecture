using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smartitecture.Services.Safety;

namespace Smartitecture
{
    /// <summary>
    /// Main application class that provides application-specific behavior to supplement the default Application class.
    /// Handles initialization of services, API hosting, and UI components.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private Window? _window;
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the App class.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the service provider for the application.
        /// </summary>
        public IServiceProvider? Services => _serviceProvider;

        /// <summary>
        /// Gets the current application instance.
        /// </summary>
        public new static App Current => (App)System.Windows.Application.Current;

        /// <summary>
        /// Invoked when the application is launched.
        /// Sets up dependency injection and initializes services.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            RegisterExceptionLogging();
            AppLog.Info("Smartitecture startup started.");

            // Set up basic configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Set up basic dependency injection
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
            });

            _serviceProvider = services.BuildServiceProvider();

            // Apply localization + theme from preferences then show Startup window
            try
            {
                var prefsService = new Smartitecture.Services.PreferencesService();
                var prefs = prefsService.Load();

                Smartitecture.Services.LocalizationManager.Apply(prefs.Language);

                var themeString = prefs.Theme ?? "Dark";
                Smartitecture.Services.AppColorTheme theme = themeString.Equals("Light", StringComparison.OrdinalIgnoreCase)
                    ? Smartitecture.Services.AppColorTheme.Light
                    : themeString.Equals("System", StringComparison.OrdinalIgnoreCase)
                        ? Smartitecture.Services.AppColorTheme.System
                        : Smartitecture.Services.AppColorTheme.Dark;
                Smartitecture.Services.ThemeManager.Apply(theme);
            }
            catch (Exception ex)
            {
                AppLog.Error("Failed to apply saved localization or theme preferences.", ex);
            }

            var main = new MainWindow();
            this.MainWindow = main;
            _window = main;
            main.Show();
            AppLog.Info("Smartitecture main window shown.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppLog.Info($"Smartitecture exited with code {e.ApplicationExitCode}.");
            base.OnExit(e);
        }

        private void RegisterExceptionLogging()
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    AppLog.Error("Unhandled app-domain exception.", ex);
                }
                else
                {
                    AppLog.Info($"Unhandled app-domain exception object: {args.ExceptionObject}");
                }
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                AppLog.Error("Unobserved task exception.", args.Exception);
                args.SetObserved();
            };
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppLog.Error("Unhandled UI exception.", e.Exception);
        }

    }
}



