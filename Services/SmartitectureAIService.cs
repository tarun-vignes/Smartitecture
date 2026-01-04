using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartitecture.Services.Interfaces;
using Smartitecture.Services.Core;
using Smartitecture.Services.Modes;

namespace Smartitecture.Services
{
    /// <summary>
    /// Main AI service that orchestrates LUMEN, FORTIS, and NEXA modes
    /// </summary>
    public class SmartitectureAIService : ILLMService
    {
        private readonly AIModeRouter _modeRouter;
        private readonly LumenService _lumenService;
        private readonly FortisService _fortisService;
        private readonly NexaService _nexaService;
        private readonly Dictionary<string, List<ConversationMessage>> _conversations;

        public string CurrentModel { get; private set; } = "LUMEN";

        public IEnumerable<string> AvailableModels => new[]
        {
            "LUMEN (💡 General Assistant)",
            "FORTIS (🛡️ Security Expert)", 
            "NEXA (⚡ Performance Optimizer)",
            "AUTO-DETECT (🔍 Smart Routing)"
        };

        public event EventHandler<ModelSwitchedEventArgs> ModelSwitched;

        public SmartitectureAIService(
            WebResearchEngine webResearch,
            KnowledgeBaseService knowledgeBase,
            ILLMService baseLLMService)
        {
            _conversations = new Dictionary<string, List<ConversationMessage>>();
            
            // Initialize AI mode services
            _lumenService = new LumenService(webResearch, knowledgeBase, baseLLMService);
            _fortisService = new FortisService(baseLLMService);
            _nexaService = new NexaService(baseLLMService);

            // Initialize router and register modes
            _modeRouter = new AIModeRouter();
            _modeRouter.RegisterMode(ChatMode.Lumen, _lumenService);
            _modeRouter.RegisterMode(ChatMode.Fortis, _fortisService);
            _modeRouter.RegisterMode(ChatMode.Nexa, _nexaService);

            // Subscribe to mode changes
            _modeRouter.ModeChanged += OnModeChanged;
        }

        public async Task<string> GetResponseAsync(string message, string conversationId = null)
        {
            try
            {
                // Store conversation history
                StoreMessage(conversationId, "user", message);

                // Process through AI mode router
                var response = await _modeRouter.ProcessQueryAsync(message, conversationId);

                // Format response based on collaboration
                var formattedResponse = FormatResponse(response);

                // Store AI response
                StoreMessage(conversationId, "assistant", formattedResponse);

                return formattedResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = $"I encountered an error: {ex.Message}";
                StoreMessage(conversationId, "assistant", errorResponse);
                return errorResponse;
            }
        }

        public void SwitchModel(string modelName)
        {
            var previousModel = CurrentModel;

            if (modelName.Contains("LUMEN"))
            {
                _modeRouter.AutoDetectionEnabled = false;
                _modeRouter.ManualMode = ChatMode.Lumen;
                CurrentModel = "LUMEN";
            }
            else if (modelName.Contains("FORTIS"))
            {
                _modeRouter.AutoDetectionEnabled = false;
                _modeRouter.ManualMode = ChatMode.Fortis;
                CurrentModel = "FORTIS";
            }
            else if (modelName.Contains("NEXA"))
            {
                _modeRouter.AutoDetectionEnabled = false;
                _modeRouter.ManualMode = ChatMode.Nexa;
                CurrentModel = "NEXA";
            }
            else if (modelName.Contains("AUTO-DETECT"))
            {
                _modeRouter.AutoDetectionEnabled = true;
                CurrentModel = "AUTO-DETECT";
            }

            // Fire model switched event
            ModelSwitched?.Invoke(this, new ModelSwitchedEventArgs(previousModel, CurrentModel));
        }

        public async Task InitializeAsync()
        {
            // Initialize all AI modes
            await _lumenService.InitializeAsync();
            await _fortisService.InitializeAsync();
            await _nexaService.InitializeAsync();
        }

        public List<ConversationMessage> GetConversationHistory(string conversationId)
        {
            return _conversations.TryGetValue(conversationId ?? "default", out var messages) 
                ? new List<ConversationMessage>(messages) 
                : new List<ConversationMessage>();
        }

        public void ClearConversationHistory(string conversationId = null)
        {
            var id = conversationId ?? "default";
            if (_conversations.ContainsKey(id))
            {
                _conversations[id].Clear();
            }
        }

        public ChatMode GetCurrentMode()
        {
            return _modeRouter.CurrentMode;
        }

        public bool IsAutoDetectionEnabled()
        {
            return _modeRouter.AutoDetectionEnabled;
        }

        public void SetAutoDetection(bool enabled)
        {
            _modeRouter.AutoDetectionEnabled = enabled;
            CurrentModel = enabled ? "AUTO-DETECT" : _modeRouter.CurrentMode.GetDisplayName();
        }

        public Dictionary<ChatMode, string> GetModeDescriptions()
        {
            var modes = _modeRouter.GetAvailableModes();
            return modes.ToDictionary(
                kvp => kvp.Key,
                kvp => $"{kvp.Value.ModeIcon} {kvp.Value.ModeName}: {kvp.Value.Description}"
            );
        }

        #region Private Methods

        private void OnModeChanged(object sender, ModeChangedEventArgs e)
        {
            if (_modeRouter.AutoDetectionEnabled)
            {
                var previousModel = CurrentModel;
                CurrentModel = $"AUTO-DETECT ({e.NewMode.GetDisplayName()})";
                ModelSwitched?.Invoke(this, new ModelSwitchedEventArgs(previousModel, CurrentModel));
            }
        }

        private string FormatResponse(AIResponse response)
        {
            if (response.CollaboratingModes?.Any() == true)
            {
                // Collaborative response
                var header = $"**{response.SourceMode}** (with {string.Join(", ", response.CollaboratingModes.Where(m => m != response.SourceMode))})";
                return $"{header}\n\n{response.Content}";
            }
            else
            {
                // Single mode response
                var mode = _modeRouter.CurrentMode;
                var header = $"**{mode.GetIcon()} {mode.GetDisplayName()}**";
                return $"{header}\n\n{response.Content}";
            }
        }

        private void StoreMessage(string conversationId, string role, string content)
        {
            var id = conversationId ?? "default";
            if (!_conversations.ContainsKey(id))
            {
                _conversations[id] = new List<ConversationMessage>();
            }

            _conversations[id].Add(new ConversationMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            });

            // Keep only last 50 messages per conversation
            if (_conversations[id].Count > 50)
            {
                _conversations[id] = _conversations[id].Skip(_conversations[id].Count - 50).ToList();
            }
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for model switching
    /// </summary>
    public class ModelSwitchedEventArgs : EventArgs
    {
        public string PreviousModel { get; }
        public string NewModel { get; }

        public ModelSwitchedEventArgs(string previousModel, string newModel)
        {
            PreviousModel = previousModel;
            NewModel = newModel;
        }
    }

    /// <summary>
    /// Conversation message for history tracking
    /// </summary>
    public class ConversationMessage
    {
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
