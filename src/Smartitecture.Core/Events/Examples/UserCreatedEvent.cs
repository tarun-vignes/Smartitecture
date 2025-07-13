using Smartitecture.Core.Events;

namespace Smartitecture.Core.Events.Examples
{
    public class UserCreatedEvent : Event
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<UserCreatedEventHandler> _logger;

        public UserCreatedEventHandler(ICacheService cacheService, ILogger<UserCreatedEventHandler> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task HandleAsync(UserCreatedEvent @event)
        {
            _logger.LogInformation("Handling UserCreatedEvent for user {UserId}", @event.UserId);

            // Example: Cache user creation event
            await _cacheService.SetAsync(
                $"user_created_{@event.UserId}",
                @event,
                TimeSpan.FromMinutes(5)
            );

            // You could also:
            // - Send welcome email
            // - Create audit log
            // - Update analytics
            // - Notify other services
        }
    }
}
