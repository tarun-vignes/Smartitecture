using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Smartitecture.Core.Configuration;
using Smartitecture.Core.Logging;
using Smartitecture.Core.Services;

namespace Smartitecture.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartitectureCore(this IServiceCollection services, IConfiguration configuration)
        {
            // Add configuration
            services.Configure<AppSettings>(configuration);
            services.AddSingleton<IConfiguration>(configuration);

            // Add database
            services.AddDbContext<SmartitectureDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add caching
            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, DistributedCacheService>();

            // Add logging
            services.AddSingleton<SmartLogger>();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            // Add event bus
            services.AddSingleton<IEventBus, EventBus>();
            services.AddScoped<IEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();

            // Add security
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped(typeof(IValidator<>), typeof(Validator<>));

            // Add validation
            services.AddScoped<IValidator<User>, UserValidator>();
            services.AddScoped<IValidator<Role>, RoleValidator>();
            services.AddScoped<IValidator<Permission>, PermissionValidator>();

            // Add services
            services.AddScoped<IRetryService, RetryService>();
            services.AddScoped<ConfigurationService>();

            // Add middleware
            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddSmartitectureSecurity(this IServiceCollection services)
        {
            // Add security services
            services.AddScoped<IPermissionManager, PermissionManager>();
            
            return services;
        }

        public static IServiceCollection AddSmartitectureOpenAI(this IServiceCollection services, IConfiguration configuration)
        {
            // Add OpenAI configuration
            services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));
            
            // Add OpenAI services
            services.AddScoped<IOpenAIService, OpenAIService>();
            
            return services;
        }

        public static IServiceCollection AddSmartitectureHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"));

            return services;
        }

        public static IServiceCollection AddSmartitectureBackgroundJobs(this IServiceCollection services)
        {
            services.AddHostedService<BackgroundJobService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            return services;
        }
    }
}
