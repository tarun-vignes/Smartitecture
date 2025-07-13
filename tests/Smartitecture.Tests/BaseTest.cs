using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Smartitecture.Core.DependencyInjection;
using Smartitecture.Core.Configuration;
using Smartitecture.Core.Logging;

namespace Smartitecture.Tests
{
    public abstract class BaseTest
    {
        protected ServiceProvider ServiceProvider { get; private set; }
        protected IConfiguration Configuration { get; private set; }
        protected ILogger<T> GetLogger<T>() => ServiceProvider.GetRequiredService<ILogger<T>>();

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Add test configuration
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            // Add core services
            services.AddSmartitectureCore(Configuration);
            services.AddSmartitectureSecurity();
            services.AddSmartitectureOpenAI(Configuration);
            services.AddSmartitectureHealthChecks();
            services.AddSmartitectureBackgroundJobs();

            // Add test-specific services
            services.RemoveAll<ILoggerProvider>();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            });

            // Add in-memory database for testing
            services.AddDbContext<SmartitectureDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            // Add test-specific implementations
            services.AddScoped<ICacheService, TestCacheService>();
            services.AddScoped<IEventBus, TestEventBus>();
        }

        protected virtual void Initialize()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }
    }

    public class TestCacheService : ICacheService
    {
        private readonly Dictionary<string, (object Value, DateTime Expiration)> _cache = new();
        private readonly ILogger<TestCacheService> _logger;

        public TestCacheService(ILogger<TestCacheService> logger)
        {
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.Expiration > DateTime.UtcNow)
            {
                return (T)entry.Value;
            }
            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var expiration = DateTime.UtcNow + (expiry ?? TimeSpan.FromMinutes(10));
            _cache[key] = (value, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            _cache.Remove(key);
        }

        public async Task ClearAsync()
        {
            _cache.Clear();
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return _cache.ContainsKey(key);
        }
    }

    public class TestEventBus : IEventBus
    {
        private readonly List<(Type EventType, object Event)> _publishedEvents = new();
        private readonly ILogger<TestEventBus> _logger;

        public TestEventBus(ILogger<TestEventBus> logger)
        {
            _logger = logger;
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            _logger.LogInformation("Subscribed to event {EventType}", typeof(TEvent).Name);
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            _logger.LogInformation("Unsubscribed from event {EventType}", typeof(TEvent).Name);
        }

        public async Task PublishAsync<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            _publishedEvents.Add((typeof(TEvent), @event));
            _logger.LogInformation("Published event {EventType}", typeof(TEvent).Name);
        }

        public List<(Type EventType, object Event)> GetPublishedEvents() => 
            new(_publishedEvents);
    }
}
