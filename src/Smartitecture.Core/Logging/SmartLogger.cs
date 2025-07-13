using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Smartitecture.Core.Logging
{
    public class SmartLogger : ILogger
    {
        private readonly ILogger _logger;
        private readonly string _categoryName;

        public SmartLogger(ILogger logger, string categoryName)
        {
            _logger = logger;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logEntry = new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Level = logLevel.ToString(),
                Category = _categoryName,
                EventId = eventId.Id,
                Message = formatter(state, exception),
                Exception = exception?.ToString(),
                CorrelationId = GetCorrelationId()
            };

            var json = JsonSerializer.Serialize(logEntry);
            _logger.Log(logLevel, eventId, json, exception, (s, e) => s);
        }

        private string GetCorrelationId()
        {
            // In a real application, this would come from a distributed tracing system
            return Activity.Current?.Id ?? Guid.NewGuid().ToString();
        }
    }

    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; }
        public string Category { get; set; }
        public int EventId { get; set; }
        public string Message { get; set; }
        public string? Exception { get; set; }
        public string CorrelationId { get; set; }
    }
}
