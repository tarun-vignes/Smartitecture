using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Core
{
    /// <summary>
    /// Routes queries to appropriate AI modes and handles collaboration between modes
    /// </summary>
    public class AIModeRouter
    {
        private readonly ConcurrentDictionary<string, List<string>> _memory = new();
        private const int MaxMemoryItems = 20;

        private readonly Dictionary<ChatMode, IAIMode> _modes;
        private Dictionary<string, List<string>> _modeKeywords;
        private bool _autoDetectionEnabled = true;
        private ChatMode _manualMode = ChatMode.Lumen;

        public event EventHandler<ModeChangedEventArgs> ModeChanged;

        public bool AutoDetectionEnabled
        {
            get => _autoDetectionEnabled;
            set => _autoDetectionEnabled = value;
        }

        public ChatMode ManualMode
        {
            get => _manualMode;
            set => _manualMode = value;
        }

        public ChatMode CurrentMode { get; private set; } = ChatMode.Lumen;

        public AIModeRouter()
        {
            _modes = new Dictionary<ChatMode, IAIMode>();
            InitializeKeywords();
        }

        public void RegisterMode(ChatMode mode, IAIMode aiMode)
        {
            _modes[mode] = aiMode;
        }

        public async Task<AIResponse> ProcessQueryAsync(string query, string conversationId = null)
        {
            var recent = GetRecentContext(conversationId);
            if (!string.IsNullOrEmpty(recent))
                query = recent + "\n" + query;
            Remember(conversationId, query);

            var targetMode = _autoDetectionEnabled ? await DetectModeFromQueryAsync(query) : _manualMode;

            if (targetMode != CurrentMode)
            {
                var prev = CurrentMode;
                CurrentMode = targetMode;
                ModeChanged?.Invoke(this, new ModeChangedEventArgs(prev, targetMode, query));
            }

            if (!_modes.TryGetValue(targetMode, out var primaryMode))
                throw new InvalidOperationException($"Mode {targetMode} is not registered");

            var context = await primaryMode.AnalyzeContextAsync(query);

            AIResponse result;
            if (ShouldCollaborate(context))
            {
                result = await HandleCollaborativeResponse(query, context, conversationId);
            }
            else
            {
                var content = await primaryMode.ProcessQueryAsync(query, conversationId);
                result = new AIResponse
                {
                    Content = SanitizeOutput(content),
                    SourceMode = targetMode.GetDisplayName(),
                    Metadata = new Dictionary<string, object>
                    {
                        ["confidence"] = context.ConfidenceScore,
                        ["mode"] = targetMode.ToString()
                    }
                };
            }

            result.Content = SanitizeOutput(result.Content ?? string.Empty);
            return result;
        }

        private void Remember(string conversationId, string text)
        {
            if (string.IsNullOrWhiteSpace(conversationId) || string.IsNullOrWhiteSpace(text)) return;
            var list = _memory.GetOrAdd(conversationId, _ => new List<string>());
            list.Add(text);
            if (list.Count > MaxMemoryItems) list.RemoveAt(0);
        }

        private string GetRecentContext(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId)) return string.Empty;
            if (!_memory.TryGetValue(conversationId, out var list) || list.Count == 0) return string.Empty;
            return string.Join("\n", list.TakeLast(5));
        }

        private async Task<ChatMode> DetectModeFromQueryAsync(string query)
        {
            var scores = new Dictionary<ChatMode, double>();
            foreach (var kvp in _modes)
            {
                var mode = kvp.Key;
                var aiMode = kvp.Value;
                var confidence = aiMode.GetConfidenceScore(query);
                var keywordBonus = CalculateKeywordScore(query, mode);
                scores[mode] = confidence + keywordBonus;
            }
            return scores.Any() ? scores.OrderByDescending(x => x.Value).First().Key : ChatMode.Lumen;
        }

        private double CalculateKeywordScore(string query, ChatMode mode)
        {
            if (!_modeKeywords.TryGetValue(mode.ToString(), out var keywords)) return 0.0;
            var q = query.ToLowerInvariant();
            var matches = keywords.Count(k => q.Contains(k));
            return matches * 0.1;
        }

        private bool ShouldCollaborate(AIContext context)
        {
            return context.RequiresSecurityAnalysis || context.RequiresPerformanceAnalysis;
        }

        private async Task<AIResponse> HandleCollaborativeResponse(string query, AIContext context, string conversationId)
        {
            var parts = new List<string>();
            var collaboratingModes = new List<string> { context.PrimaryMode };

            if (Enum.TryParse<ChatMode>(context.PrimaryMode, true, out var primaryKey) &&
                _modes.TryGetValue(primaryKey, out var primaryMode))
            {
                var primary = await primaryMode.ProcessQueryAsync(query, conversationId);
                parts.Add(primary);
            }

            if (context.RequiresSecurityAnalysis && _modes.TryGetValue(ChatMode.Fortis, out var fortis))
            {
                var sec = await fortis.ProcessQueryAsync(query, conversationId);
                parts.Add("[Security Analysis]\n" + sec);
                collaboratingModes.Add("FORTIS");
            }

            if (context.RequiresPerformanceAnalysis && _modes.TryGetValue(ChatMode.Nexa, out var nexa))
            {
                var perf = await nexa.ProcessQueryAsync(query, conversationId);
                parts.Add("[Performance Analysis]\n" + perf);
                collaboratingModes.Add("NEXA");
            }

            var combined = SanitizeOutput(string.Join("\n\n", parts));
            return new AIResponse
            {
                Content = combined,
                SourceMode = context.PrimaryMode,
                CollaboratingModes = collaboratingModes,
                Metadata = new Dictionary<string, object>
                {
                    ["collaboration"] = true,
                    ["modes"] = collaboratingModes
                }
            };
        }

        private void InitializeKeywords()
        {
            _modeKeywords = new Dictionary<string, List<string>>
            {
                ["Lumen"] = new List<string>{
                    "find","search","help","what","how","where","when","explain",
                    "file","document","folder","open","create","web","google","research" },
                ["Fortis"] = new List<string>{
                    "virus","malware","threat","security","scan","antivirus","firewall",
                    "protect","safe","danger","suspicious","hack","breach","vulnerability",
                    "encrypt","password","backup","recover","restore","quarantine" },
                ["Nexa"] = new List<string>{
                    "performance","speed","slow","fast","optimize","cpu","gpu","memory","ram",
                    "disk","temperature","fan","cooling","benchmark","fps","lag","freeze",
                    "crash","system","hardware","monitor","overclock","power","battery" }
            };
        }

        private static string SanitizeOutput(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var sb = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                if (!char.IsControl(ch) || ch == '\n' || ch == '\r' || ch == '\t')
                {
                    if (ch == '\uFFFD') continue; // skip replacement char
                    sb.Append(ch);
                }
            }
            return sb.ToString()
                .Replace("dY", string.Empty)
                .Replace("�", string.Empty)
                .Trim();
        }

        public Dictionary<ChatMode, IAIMode> GetRegisteredModes() => new Dictionary<ChatMode, IAIMode>(_modes);
    }

    /// <summary>
    /// Event arguments for mode change events
    /// </summary>
    public class ModeChangedEventArgs : EventArgs
    {
        public ChatMode PreviousMode { get; }
        public ChatMode NewMode { get; }
        public string TriggerQuery { get; }

        public ModeChangedEventArgs(ChatMode previousMode, ChatMode newMode, string triggerQuery)
        {
            PreviousMode = previousMode;
            NewMode = newMode;
            TriggerQuery = triggerQuery;
        }
    }
}
