using AIPal.Services.Handlers;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace AIPal.Services
{
    /// <summary>
    /// Extension methods for configuring services in the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds agent-related services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure options
            services.Configure<AgentOptions>(configuration.GetSection("Agent"));
            services.Configure<AzureOpenAIOptions>(configuration.GetSection("AzureOpenAI"));

            // Register Azure OpenAI client
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureOpenAIOptions>>().Value;
                var logger = provider.GetRequiredService<ILogger<OpenAIClient>>();

                if (string.IsNullOrEmpty(options.Endpoint) || string.IsNullOrEmpty(options.ApiKey))
                {
                    logger.LogWarning("Azure OpenAI endpoint or API key not configured. Using mock client.");
                    return new OpenAIClient(
                        new Uri("https://mock.openai.azure.com"),
                        new AzureKeyCredential("mock-key"));
                }

                logger.LogInformation("Initializing Azure OpenAI client with endpoint: {Endpoint}", options.Endpoint);
                return new OpenAIClient(
                    new Uri(options.Endpoint),
                    new AzureKeyCredential(options.ApiKey));
            });

            // Register agent services
            services.AddSingleton<IAgentService, AzureOpenAIAgentService>();
            services.AddSingleton<ToolRegistrationService>();
            
            // Register request handlers
            RegisterAgentRequestHandlers(services);

            return services;
        }
        
        /// <summary>
        /// Initializes the agent system by registering tools and setting up initial state.
        /// This should be called during application startup after services are built.
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        public static void InitializeAgentSystem(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ToolRegistrationService>>();
            logger.LogInformation("Initializing agent system");
            
            // Get the tool registration service
            var toolRegistrationService = serviceProvider.GetRequiredService<ToolRegistrationService>();
            
            // Get the agent service
            var agentService = serviceProvider.GetRequiredService<IAgentService>();
            
            // Register tools with the agent service
            toolRegistrationService.RegisterTools(agentService);
            
            logger.LogInformation("Agent system initialized successfully");
        }
        
        /// <summary>
        /// Registers all agent request handlers with the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection</param>
        private static void RegisterAgentRequestHandlers(IServiceCollection services)
        {
            // Register specific handlers
            services.AddTransient<SystemInfoRequestHandler>();
            services.AddTransient<FileSearchRequestHandler>();
            services.AddTransient<LaunchAppRequestHandler>();
            services.AddTransient<WebSearchRequestHandler>();
            services.AddTransient<WeatherRequestHandler>();
            services.AddTransient<SystemDiagnosticsHandler>();
            services.AddTransient<NetworkSecurityHandler>();
            services.AddTransient<SecurityHandler>();
            services.AddTransient<ScreenAnalysisHandler>();
            
            // Register default handler
            services.AddTransient<DefaultRequestHandler>();
        }
    }
}
