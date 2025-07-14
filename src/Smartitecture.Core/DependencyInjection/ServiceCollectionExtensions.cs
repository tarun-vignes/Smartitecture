using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smartitecture.Core.Services;
using System.Net.Http;

namespace Smartitecture.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartitectureCore(this IServiceCollection services)
        {
            // Add HTTP client for Python API
            services.AddHttpClient<PythonApiService>();
            
            // Add our services
            services.AddSingleton<PythonApiService>();
            services.AddHostedService<PythonBackendService>();
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            return services;
        }
    }
}
