using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smartitecture.Core.DependencyInjection;
using Smartitecture.Core.Configuration;
using Smartitecture.Core.Logging;
using Smartitecture.Core.Security;
using System.Windows.Forms;

namespace Smartitecture.Desktop
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add core services
                    services.AddSmartitectureCore(context.Configuration);
                    services.AddSmartitectureSecurity();
                    services.AddSmartitectureOpenAI(context.Configuration);
                    services.AddSmartitectureHealthChecks();
                    services.AddSmartitectureBackgroundJobs();

                    // Add desktop-specific services
                    services.AddSingleton<MainForm>();
                    services.AddSingleton<ITrayIconService, TrayIconService>();
                    services.AddSingleton<IWindowManager, WindowManager>();
                })
                .Build();

            // Get the main form from the service provider
            var mainForm = host.Services.GetRequiredService<MainForm>();
            
            // Run the application
            Application.Run(mainForm);
        }
    }
}
