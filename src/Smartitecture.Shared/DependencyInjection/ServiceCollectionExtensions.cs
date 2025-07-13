using Microsoft.Extensions.DependencyInjection;

namespace Smartitecture.Shared.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartitectureShared(this IServiceCollection services)
        {
            services.AddSingleton<IHandlerRegistry, HandlerRegistry>();
            return services;
        }
    }
}
