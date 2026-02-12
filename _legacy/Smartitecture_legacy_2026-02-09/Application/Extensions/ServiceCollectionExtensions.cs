using System;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Smartitecture.Application.Models;
using Smartitecture.Application.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Azure OpenAI services in an <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Azure OpenAI services to the specified <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureOptions">A delegate to configure the <see cref="AzureOpenAIOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddAzureOpenAI(this IServiceCollection services, Action<AzureOpenAIOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            // Register configuration
            services.Configure(configureOptions);

            // Register Azure OpenAI client
            services.AddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
                
                if (string.IsNullOrEmpty(options.Endpoint))
                {
                    throw new InvalidOperationException($"{nameof(AzureOpenAIOptions.Endpoint)} must be configured.");
                }

                if (string.IsNullOrEmpty(options.ApiKey))
                {
                    throw new InvalidOperationException($"{nameof(AzureOpenAIOptions.ApiKey)} must be configured.");
                }

                var client = new OpenAIClient(
                    new Uri(options.Endpoint),
                    new AzureKeyCredential(options.ApiKey));

                return client;
            });

            // Register agent service
            services.AddScoped<IAgentService, AzureOpenAIAgentService>();

            return services;
        }
    }
}
