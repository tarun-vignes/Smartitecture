using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            // Show Startup window first
            var startup = new StartupWindow();
            this.MainWindow = startup;
            _window = startup;
            startup.Show();
        }

    }
}



