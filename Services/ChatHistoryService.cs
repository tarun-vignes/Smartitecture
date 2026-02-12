using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Smartitecture.Services
{
    public class ChatHistoryMessage
    {
        public string Role { get; set; } = "assistant";
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ChatHistorySession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "New Chat";
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<ChatHistoryMessage> Messages { get; set; } = new();
    }

    public class ChatHistoryService
    {
        private readonly string _historyPath;
        private readonly object _lock = new();
        private List<ChatHistorySession> _cache;

        public ChatHistoryService()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smartitecture");
            Directory.CreateDirectory(dir);
            _historyPath = Path.Combine(dir, "chat_history.json");
            _cache = LoadInternal();
        }

        public IReadOnlyList<ChatHistorySession> GetSessions()
        {
            lock (_lock)
            {
                PurgeExpiredDeletions();
                return _cache
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.LastUpdated)
                    .ToList();
            }
        }

        public ChatHistorySession? GetSession(string id)
        {
            lock (_lock)
            {
                var session = _cache.FirstOrDefault(s => s.Id == id);
                if (session == null || session.IsDeleted)
                {
                    return null;
                }

                return session;
            }
        }

        public void StartSession(string sessionId)
        {
            lock (_lock)
            {
                if (_cache.Any(s => s.Id == sessionId))
                {
                    return;
                }

                _cache.Add(new ChatHistorySession
                {
                    Id = sessionId,
                    Title = "New Chat",
                    StartedAt = DateTime.Now,
                    LastUpdated = DateTime.Now
                });
                SaveInternal();
            }
        }

        public void AppendMessage(string sessionId, string role, string content)
        {
            lock (_lock)
            {
                var session = _cache.FirstOrDefault(s => s.Id == sessionId);
                if (session == null)
                {
                    session = new ChatHistorySession
                    {
                        Id = sessionId,
                        Title = "New Chat",
                        StartedAt = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    _cache.Add(session);
                }
                else if (session.IsDeleted)
                {
                    session.IsDeleted = false;
                    session.DeletedAt = null;
                }

                session.Messages.Add(new ChatHistoryMessage
                {
                    Role = role,
                    Content = content,
                    Timestamp = DateTime.Now
                });
                session.LastUpdated = DateTime.Now;

                if (role.Equals("user", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(session.Title, "New Chat", StringComparison.OrdinalIgnoreCase))
                {
                    session.Title = Truncate(content, 48);
                }

                SaveInternal();
            }
        }

        public void MarkDeleted(string sessionId)
        {
            lock (_lock)
            {
                var session = _cache.FirstOrDefault(s => s.Id == sessionId);
                if (session == null)
                {
                    return;
                }

                session.IsDeleted = true;
                session.DeletedAt = DateTime.Now;
                SaveInternal();
            }
        }

        private List<ChatHistorySession> LoadInternal()
        {
            try
            {
                if (File.Exists(_historyPath))
                {
                    var json = File.ReadAllText(_historyPath);
                    var sessions = JsonSerializer.Deserialize<List<ChatHistorySession>>(json);
                    if (sessions != null)
                    {
                        return sessions;
                    }
                }
            }
            catch
            {
            }

            return new List<ChatHistorySession>();
        }

        private void SaveInternal()
        {
            try
            {
                var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_historyPath, json);
            }
            catch
            {
            }
        }

        private void PurgeExpiredDeletions()
        {
            var cutoff = DateTime.Now.AddDays(-30);
            var removed = _cache.RemoveAll(s => s.IsDeleted && s.DeletedAt.HasValue && s.DeletedAt.Value < cutoff);
            if (removed > 0)
            {
                SaveInternal();
            }
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "New Chat";
            }

            var clean = text.Trim();
            if (clean.Length <= maxLength)
            {
                return clean;
            }

            return clean.Substring(0, maxLength).TrimEnd() + "...";
        }
    }
}
