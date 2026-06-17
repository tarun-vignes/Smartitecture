using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using Smartitecture.Services;
using Smartitecture.Services.Automation;
using Smartitecture.Services.Core;

namespace Smartitecture.UI
{
    public partial class ChatView : UserControl
    {
        // Core services (chat models, history, tool execution).
        private readonly ILLMService _llmService = null!;
        private readonly ChatHistoryService _historyService = new ChatHistoryService();
        private readonly ToolExecutionService _toolExecutor = new ToolExecutionService();
        private readonly VoiceInputService _voiceInput = new VoiceInputService();
        // Active conversation state.
        private string _conversationId;
        // UI/interaction state.
        private readonly DispatcherTimer _typingTimer;
        private bool _isProcessing = false;
        private bool _isReplayingHistory = false;
        private bool _hasUserMessages = false;
        private bool _previewingDeleted = false;
        private string? _pendingDeleteId;
        private DeleteConfirmMode _deleteConfirmMode = DeleteConfirmMode.Soft;
        private ToolCall? _pendingToolConfirmation;
        private int _providerStatusCheckId = 0;
        private string? _lastAssistantMessage;
        private ToolMemory? _lastToolMemory;

        private enum DeleteConfirmMode
        {
            Soft,
            Permanent
        }

        public ChatView(ILLMService llmService)
        {
            InitializeComponent();
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _conversationId = Guid.NewGuid().ToString();

            // Typing indicator animation timer.
            _typingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _typingTimer.Tick += TypingTimer_Tick;

            // Placeholder behavior.
            SetupPlaceholderText();

            Loaded += ChatView_Loaded;
            Unloaded += ChatView_Unloaded;
        }

        private async void ChatView_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            MessageInput.Focus();
            await RefreshProviderStatusAsync();
        }

        private void ChatView_Unloaded(object sender, RoutedEventArgs e)
        {
            _typingTimer?.Stop();
        }

        private void GoDashboard_Click(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoDashboard();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Alt)
            {
                if (e.Key == System.Windows.Input.Key.Left)
                {
                    GoDashboard_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.H)
                {
                    GoHome_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.S)
                {
                    OpenSettings_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }

        private void SetupPlaceholderText()
        {
            // Manual placeholder to keep glass textbox styling consistent.
            var placeholderText = GetString("Chat.Placeholder", "Type your message or command here...");
            MessageInput.Text = placeholderText;
            MessageInput.Foreground = GetPlaceholderBrush();

            MessageInput.GotFocus += (s, e) =>
            {
                if (MessageInput.Text == placeholderText)
                {
                    MessageInput.Text = "";
                    MessageInput.Foreground = GetOnSurfaceBrush();
                }
            };

            MessageInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(MessageInput.Text))
                {
                    MessageInput.Text = placeholderText;
                    MessageInput.Foreground = GetPlaceholderBrush();
                }
            };
        }

        private Brush GetOnSurfaceBrush()
        {
            return TryFindResource("Brush.OnSurface") as Brush ?? Brushes.White;
        }

        private Brush GetPlaceholderBrush()
        {
            return TryFindResource("Brush.Placeholder") as Brush ?? SystemColors.GrayTextBrush;
        }

        private Brush GetChatUserForeground()
        {
            return TryFindResource("Brush.ChatBubbleUserForeground") as Brush ?? GetOnSurfaceBrush();
        }

        private Brush GetChatAssistantForeground()
        {
            return TryFindResource("Brush.ChatBubbleAssistantForeground") as Brush ?? GetOnSurfaceBrush();
        }

        private Brush GetChatSystemForeground()
        {
            return TryFindResource("Brush.ChatBubbleSystemForeground") as Brush ?? GetOnSurfaceBrush();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            var placeholderText = GetString("Chat.Placeholder", "Type your message or command here...");
            if (_isProcessing || string.IsNullOrWhiteSpace(MessageInput.Text) || MessageInput.Text == placeholderText)
                return;

            var userMessage = MessageInput.Text.Trim();
            MessageInput.Text = string.Empty;
            MessageInput.Foreground = GetOnSurfaceBrush();
            _isProcessing = true;
            SendButton.IsEnabled = false;

            try
            {
                // Add user message bubble and save to history.
                AddMessageToChat(userMessage, "user");

                // Show typing indicator while processing.
                ShowTypingIndicator();

                if (ShouldRefreshProcessListForCloseRequest(userMessage))
                {
                    await ExecuteCommandAsync(
                        "list_processes",
                        new Dictionary<string, object> { ["count"] = 12 },
                        userMessage);
                    return;
                }

                var contextualFollowUp = TryBuildContextualFollowUp(userMessage);
                if (!string.IsNullOrWhiteSpace(contextualFollowUp))
                {
                    AddMessageToChat(contextualFollowUp, "assistant");
                    return;
                }

                // Check if the message maps to a system command.
                var (commandName, parameters) = await _llmService.ParseCommandAsync(userMessage);

                if (!string.IsNullOrEmpty(commandName))
                {
                    // Execute command
                    await ExecuteCommandAsync(commandName.ToLower(), parameters, userMessage);
                }
                else
                {
                    // Get AI response
                    await GetAIResponseAsync(userMessage);
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"Something went wrong: {ex.Message}", "system");
            }
            finally
            {
                HideTypingIndicator();
                _isProcessing = false;
                SendButton.IsEnabled = true;
                MessageInput.Focus();
            }
        }

        private async Task ExecuteCommandAsync(string commandName, Dictionary<string, object>? parameters, string originalMessage)
        {
            try
            {
                // Commands execute locally via ToolExecutionService.
                AddMessageToChat(BuildCommandStatusMessage(commandName), "system");

                var toolArgs = parameters?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
                var result = await _toolExecutor.ExecuteToolAsync(commandName, toolArgs);

                if (result.RequiresConfirmation)
                {
                    // For sensitive actions, show confirmation before execution.
                    var argsJson = toolArgs == null ? "{}" : System.Text.Json.JsonSerializer.Serialize(toolArgs);
                    ShowToolConfirmation(new ToolCall { Name = commandName, ArgumentsJson = argsJson });
                    return;
                }

                if (result.Success)
                {
                    RememberToolResult(commandName, result.Message, originalMessage);

                    if (ShouldShowToolResultAsAssistant(commandName))
                    {
                        AddMessageToChat(BuildToolAssistantMessage(commandName, toolArgs, result.Message, originalMessage), "assistant");
                        return;
                    }

                    AddMessageToChat(result.Message, "system");

                    // Ask the AI to summarize what happened.
                    var aiResponse = await _llmService.GetResponseAsync(
                        $"I just executed the {commandName} command for the user. Please provide a brief, helpful response about what was done.",
                        _conversationId);

                    AddMessageToChat(aiResponse, "assistant");
                }
                else
                {
                    if (ShouldShowToolResultAsAssistant(commandName))
                    {
                        AddMessageToChat(result.Message, "assistant");
                        return;
                    }

                    AddMessageToChat(result.Message, "system");
                    // Ask the AI for troubleshooting guidance.
                    var aiResponse = await _llmService.GetResponseAsync(
                        $"The {commandName} command failed. Please help the user understand what might have gone wrong and suggest alternatives.",
                        _conversationId);

                    AddMessageToChat(aiResponse, "assistant");
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"I could not complete that action: {ex.Message}", "system");
            }
        }

        private static string BuildCommandStatusMessage(string commandName)
        {
            return commandName.ToLowerInvariant() switch
            {
                "system_info" => "Checking this PC...",
                "performance_snapshot" => "Checking performance...",
                "list_processes" => "Reading running processes...",
                "network_adapters" => "Checking network adapters...",
                "battery_status" => "Checking battery status...",
                "defender_status" => "Checking Windows security...",
                "defender_scan_status" => "Checking Defender scan results...",
                "defender_scan" => "Preparing Windows Defender scan...",
                "kill_process" => "Preparing to stop the process...",
                "launch" => "Opening app...",
                "calculator" => "Opening Calculator...",
                "explorer" => "Opening File Explorer...",
                "taskmgr" => "Opening Task Manager...",
                _ => "Working on that..."
            };
        }

        private static bool ShouldShowToolResultAsAssistant(string commandName)
        {
            return commandName.Equals("system_info", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("performance_snapshot", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("list_processes", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("network_adapters", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("battery_status", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("defender_status", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("defender_scan_status", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("defender_scan", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("launch", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("calculator", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("explorer", StringComparison.OrdinalIgnoreCase) ||
                   commandName.Equals("taskmgr", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildToolAssistantMessage(string commandName, Dictionary<string, object?>? args, string fallback, string originalMessage)
        {
            if (commandName.Equals("system_info", StringComparison.OrdinalIgnoreCase))
            {
                return $"Here is the basic profile for this PC.\n\n{fallback}";
            }

            if (commandName.Equals("performance_snapshot", StringComparison.OrdinalIgnoreCase))
            {
                return $"{BuildPerformanceSummary(fallback, originalMessage)}\n\n{fallback}";
            }

            if (commandName.Equals("list_processes", StringComparison.OrdinalIgnoreCase))
            {
                if (IsCloseRecommendationRequest(originalMessage.ToLowerInvariant()))
                {
                    return BuildCloseRecommendation(fallback);
                }

                return $"These are the processes using the most memory right now.\n\n{fallback}";
            }

            if (commandName.Equals("network_adapters", StringComparison.OrdinalIgnoreCase))
            {
                return $"Here are the active network adapters and addresses I can see.\n\n{fallback}";
            }

            if (commandName.Equals("battery_status", StringComparison.OrdinalIgnoreCase))
            {
                return $"Here is the current battery information I can read.\n\n{fallback}";
            }

            if (commandName.Equals("defender_status", StringComparison.OrdinalIgnoreCase))
            {
                return $"{BuildDefenderSummary(fallback)}\n\n{fallback}";
            }

            if (commandName.Equals("defender_scan_status", StringComparison.OrdinalIgnoreCase))
            {
                return fallback;
            }

            if (commandName.Equals("defender_scan", StringComparison.OrdinalIgnoreCase))
            {
                return fallback;
            }

            if (commandName.Equals("launch", StringComparison.OrdinalIgnoreCase))
            {
                var target = ExtractToolArg(args, "target") ??
                             ExtractToolArg(args, "app") ??
                             ExtractToolArg(args, "application") ??
                             "the app";

                return $"Opened {target}.";
            }

            if (commandName.Equals("calculator", StringComparison.OrdinalIgnoreCase))
            {
                return "Opened Calculator.";
            }

            if (commandName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
            {
                return "Opened File Explorer.";
            }

            if (commandName.Equals("taskmgr", StringComparison.OrdinalIgnoreCase))
            {
                return "Opened Task Manager.";
            }

            return fallback;
        }

        private static string BuildPerformanceSummary(string output, string originalMessage)
        {
            var question = originalMessage.ToLowerInvariant();
            var askedAboutHeat = ContainsAny(question, "hot", "heat", "overheat", "overheating", "fan", "temperature");
            var askedAboutSlowness = ContainsAny(question, "slow", "lag", "laggy", "freezing", "stutter", "performance");
            var cpu = ExtractPercent(output, "CPU load:");
            var memory = ExtractMemoryUsedPercent(output);

            if (askedAboutHeat)
            {
                var reasons = new List<string>();
                if (cpu.HasValue && cpu.Value >= 75)
                {
                    reasons.Add($"CPU load is high at {cpu.Value:0.#}%, which can make the machine run hot and spin up fans.");
                }

                if (memory.HasValue && memory.Value >= 85)
                {
                    reasons.Add($"Memory usage is high at {memory.Value:0.#}%, which can increase overall system pressure.");
                }

                if (reasons.Count == 0)
                {
                    reasons.Add("I do not have direct temperature sensor access in this build, and this snapshot does not show an obvious CPU or memory spike.");
                    reasons.Add("If it still feels hot, check vents/fans, use a hard surface, close heavy apps, and look for dust or blocked airflow.");
                }

                return "Here is the likely heat picture from what I can read locally:\n- " + string.Join("\n- ", reasons);
            }

            if (askedAboutSlowness)
            {
                var reasons = new List<string>();
                if (cpu.HasValue && cpu.Value >= 85)
                {
                    reasons.Add($"CPU load is very high at {cpu.Value:0.#}%, so the top processes below are the first suspects.");
                }
                else if (cpu.HasValue && cpu.Value >= 60)
                {
                    reasons.Add($"CPU load is moderately high at {cpu.Value:0.#}%, which can cause sluggishness while apps are busy.");
                }

                if (memory.HasValue && memory.Value >= 85)
                {
                    reasons.Add($"Memory usage is high at {memory.Value:0.#}%, so Windows may be compressing memory or paging to disk.");
                }
                else if (memory.HasValue && memory.Value >= 70)
                {
                    reasons.Add($"Memory usage is elevated at {memory.Value:0.#}%; closing heavy apps may help.");
                }

                if (reasons.Count == 0)
                {
                    reasons.Add("CPU and memory do not show an obvious bottleneck in this snapshot.");
                    reasons.Add("If the PC still feels slow, the next checks are disk activity, startup apps, browser tabs/extensions, updates, and thermal throttling.");
                }

                return "Here is why the PC may feel slow right now:\n- " + string.Join("\n- ", reasons);
            }

            if (cpu.HasValue)
            {
                if (cpu.Value >= 85)
                {
                    return "Your CPU is under heavy load right now. The top process list below is the first place to look.";
                }

                if (cpu.Value >= 60)
                {
                    return "Your CPU is moderately busy. If the PC feels slow, check the top memory and CPU-related processes below.";
                }

                return "Your CPU load looks normal from this snapshot. If the PC still feels slow, memory, disk, startup apps, or heat may be involved.";
            }

            return "Here is a current performance snapshot. CPU load was not available, so use the process and memory details below as the main signal.";
        }

        private static bool ContainsAny(string value, params string[] patterns)
        {
            return patterns.Any(pattern => value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private string? TryBuildContextualFollowUp(string userMessage)
        {
            var normalized = (userMessage ?? string.Empty).Trim().ToLowerInvariant();
            if (!IsShortContextualFollowUp(normalized) || (_lastToolMemory == null && string.IsNullOrWhiteSpace(_lastAssistantMessage)))
            {
                return null;
            }

            if (_lastToolMemory != null)
            {
                var structured = TryBuildStructuredToolFollowUp(normalized, _lastToolMemory);
                if (!string.IsNullOrWhiteSpace(structured))
                {
                    return structured;
                }
            }

            var previous = _lastAssistantMessage ?? string.Empty;
            var previousLower = previous.ToLowerInvariant();
            var memory = ExtractMemoryUsedPercent(previous);
            var cpu = ExtractPercent(previous, "CPU load:");
            var asksWhatToClose = IsCloseRecommendationRequest(normalized);

            if (asksWhatToClose)
            {
                return BuildCloseRecommendation(previous);
            }

            if (ContainsAny(previousLower, "memory usage is elevated", "memory usage is high", "ram", "memory:", "top memory processes"))
            {
                var details = new List<string>();
                if (memory.HasValue)
                {
                    details.Add($"Memory is at {memory.Value:0.#}% used. That means apps and Windows are competing for working space.");
                }
                else
                {
                    details.Add("The previous result pointed at memory pressure, which can make the system feel slower even when CPU is not high.");
                }

                details.Add("When memory gets crowded, Windows may compress memory or move data to disk, and disk is much slower than RAM.");
                details.Add("The top memory processes are the first things to inspect. Closing heavy apps, browser tabs, or extra editor windows should help if you do not need them open.");

                return "Because the last performance check points more toward memory pressure than CPU pressure:\n- " + string.Join("\n- ", details);
            }

            if (ContainsAny(previousLower, "cpu load is high", "cpu load is very high", "cpu load is moderately high", "cpu load:"))
            {
                var details = new List<string>();
                if (cpu.HasValue)
                {
                    details.Add($"CPU load was {cpu.Value:0.#}%. Sustained CPU load makes apps wait longer for processor time.");
                }
                else
                {
                    details.Add("The previous result pointed at CPU load, which can make the machine feel laggy while apps compete for processor time.");
                }

                details.Add("High CPU can also raise temperature, which may trigger fan noise or thermal throttling.");
                details.Add("The next step is checking top CPU processes and closing, updating, or restarting the app causing the load.");

                return "Because CPU pressure can directly slow down the system:\n- " + string.Join("\n- ", details);
            }

            if (ContainsAny(previousLower, "network adapters", "ip:", "wi-fi", "wifi", "ethernet"))
            {
                return "Because each adapter can represent a different connection path. The active adapter with a normal IP address is usually the one carrying traffic. Down adapters or link-local addresses can show disconnected hardware, virtual adapters, or connections that are not reaching your router.";
            }

            if (ContainsAny(previousLower, "defender", "antivirus", "windows security"))
            {
                return "Because security status tells us whether Windows has active protection available. If Defender is disabled, out of date, or blocked, scans and real-time protection may not catch threats reliably.";
            }

            return "Because the previous result was based on local signals from this PC. If you want, ask a more specific follow-up like \"why memory?\", \"why CPU?\", or \"what should I close?\" and I can narrow it down from the last check.";
        }

        private bool ShouldRefreshProcessListForCloseRequest(string userMessage)
        {
            var normalized = (userMessage ?? string.Empty).Trim().ToLowerInvariant();
            if (!IsCloseRecommendationRequest(normalized))
            {
                return false;
            }

            if (_lastToolMemory == null)
            {
                return true;
            }

            return !_lastToolMemory.ToolName.Equals("performance_snapshot", StringComparison.OrdinalIgnoreCase) &&
                   !_lastToolMemory.ToolName.Equals("list_processes", StringComparison.OrdinalIgnoreCase);
        }

        private string? TryBuildStructuredToolFollowUp(string normalized, ToolMemory memory)
        {
            if (IsCloseRecommendationRequest(normalized))
            {
                return BuildCloseRecommendation(memory);
            }

            if (IsCloseBiggestRequest(normalized))
            {
                return PrepareCloseBiggestProcess(memory);
            }

            if (ContainsAny(normalized, "is that normal", "is this normal", "normal?"))
            {
                return BuildNormalityCheck(memory);
            }

            if (memory.ToolName.Equals("performance_snapshot", StringComparison.OrdinalIgnoreCase) ||
                memory.ToolName.Equals("list_processes", StringComparison.OrdinalIgnoreCase))
            {
                return BuildPerformanceWhy(memory);
            }

            if (memory.ToolName.Equals("network_adapters", StringComparison.OrdinalIgnoreCase))
            {
                return "Because each adapter can represent a different connection path. The active adapter with a normal IP address is usually the one carrying traffic. Down adapters or link-local addresses can show disconnected hardware, virtual adapters, or connections that are not reaching your router.";
            }

            if (memory.ToolName.Equals("defender_status", StringComparison.OrdinalIgnoreCase))
            {
                return "Because security status tells us whether Windows has active antivirus protection registered and available. If protection is disabled, out of date, or blocked, scans and real-time protection may not catch threats reliably.";
            }

            return null;
        }

        private static string BuildPerformanceWhy(ToolMemory memory)
        {
            var details = new List<string>();
            if (memory.MemoryUsedPercent.HasValue && memory.MemoryUsedPercent.Value >= 70)
            {
                details.Add($"Memory is at {memory.MemoryUsedPercent.Value:0.#}% used. That means apps and Windows are competing for working space.");
                details.Add("When memory gets crowded, Windows may compress memory or move data to disk, and disk is much slower than RAM.");
            }

            if (memory.CpuPercent.HasValue && memory.CpuPercent.Value >= 60)
            {
                details.Add($"CPU load was {memory.CpuPercent.Value:0.#}%. Sustained CPU load makes apps wait longer for processor time.");
            }

            if (memory.Processes.Count > 0)
            {
                var topUserProcess = memory.Processes.FirstOrDefault(process => !IsSystemProcess(process.Name));
                if (topUserProcess != null)
                {
                    details.Add($"{topUserProcess.Name} is the highest user-app memory consumer I saw at {topUserProcess.Memory}.");
                }
            }

            if (details.Count == 0)
            {
                details.Add("The last local snapshot did not show an obvious CPU or memory bottleneck.");
                details.Add("The next likely causes are disk activity, startup apps, browser extensions, Windows updates, or heat/thermal throttling.");
            }

            return "Because the last tool result points to these local signals:\n- " + string.Join("\n- ", details);
        }

        private static string BuildNormalityCheck(ToolMemory memory)
        {
            var notes = new List<string>();
            if (memory.MemoryUsedPercent.HasValue)
            {
                if (memory.MemoryUsedPercent.Value >= 85)
                {
                    notes.Add($"Memory at {memory.MemoryUsedPercent.Value:0.#}% is high. It can be normal during heavy work, but it is worth reducing open apps if the PC feels slow.");
                }
                else if (memory.MemoryUsedPercent.Value >= 70)
                {
                    notes.Add($"Memory at {memory.MemoryUsedPercent.Value:0.#}% is elevated but not automatically dangerous.");
                }
                else
                {
                    notes.Add($"Memory at {memory.MemoryUsedPercent.Value:0.#}% looks normal.");
                }
            }

            if (memory.CpuPercent.HasValue)
            {
                notes.Add(memory.CpuPercent.Value >= 80
                    ? $"CPU at {memory.CpuPercent.Value:0.#}% is high if it stays there."
                    : $"CPU at {memory.CpuPercent.Value:0.#}% looks reasonable for a momentary snapshot.");
            }

            if (memory.Processes.Any(process => process.Name.Contains("Memory Compression", StringComparison.OrdinalIgnoreCase)))
            {
                notes.Add("Memory Compression is a normal Windows process. Do not close it.");
            }

            if (notes.Count == 0)
            {
                notes.Add("I do not have enough structured signals from the last tool result to call it normal or abnormal.");
            }

            return string.Join(Environment.NewLine, notes);
        }

        private string PrepareCloseBiggestProcess(ToolMemory memory)
        {
            var candidate = GetSafeCloseCandidates(memory).FirstOrDefault();
            if (candidate == null)
            {
                return "I do not see a safe user-app candidate to close from the last tool result. I will not suggest stopping Windows/system processes.";
            }

            if (!candidate.Pid.HasValue)
            {
                return $"The biggest safe user-app candidate is {candidate.Name} ({candidate.Memory}), but I do not have its PID from the last result. Type \"close {candidate.Name}\" if you want me to prepare a confirmed stop action.";
            }

            var argsJson = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["pid"] = candidate.Pid.Value,
                ["name"] = candidate.Name
            });

            ShowToolConfirmation(new ToolCall { Name = "kill_process", ArgumentsJson = argsJson });
            return $"The biggest safe user-app candidate is {candidate.Name} ({candidate.Memory}, PID {candidate.Pid}). I opened a confirmation prompt before stopping it.";
        }

        private static string BuildCloseRecommendation(string previous)
        {
            var processes = ExtractProcessSummaries(previous)
                .Where(IsSafeCloseCandidate)
                .Take(5)
                .ToList();

            if (processes.Count == 0)
            {
                return "I do not see a safe user-app candidate in the last process list. Avoid closing Windows/system items like Memory Compression, Defender, service hosts, drivers, or virtual machine services unless you know exactly what they are doing.";
            }

            var lines = new List<string>
            {
                "Based on the last memory list, close or reduce these first if you are not actively using them:"
            };

            foreach (var process in processes)
            {
                lines.Add($"- {process.Name} ({process.Memory})");
            }

            lines.Add("");
            lines.Add("Do not force-close Windows/system processes. Close apps normally first. For Code, close unused windows/extensions. For browsers, close heavy tabs. For Discord or Opera/Chrome, quit them if you do not need them right now.");
            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildCloseRecommendation(ToolMemory memory)
        {
            var processes = GetSafeCloseCandidates(memory)
                .Take(5)
                .ToList();

            if (processes.Count == 0)
            {
                return "I do not see a safe user-app candidate in the last tool result. Avoid closing Windows/system items like Memory Compression, Defender, service hosts, drivers, or virtual machine services unless you know exactly what they are doing.";
            }

            var lines = new List<string>
            {
                "Based on the last tool result, close or reduce these first if you are not actively using them:"
            };

            foreach (var process in processes)
            {
                var pid = process.Pid.HasValue ? $", PID {process.Pid}" : string.Empty;
                lines.Add($"- {process.Name} ({process.Memory}{pid})");
            }

            lines.Add("");
            lines.Add("Close apps normally first. For Code, close unused windows/extensions. For browsers, close heavy tabs. For Discord or Opera/Chrome, quit them if you do not need them right now. If you ask me to stop one, I will ask for confirmation first.");
            return string.Join(Environment.NewLine, lines);
        }

        private void RememberToolResult(string commandName, string output, string originalMessage)
        {
            _lastToolMemory = new ToolMemory(
                commandName,
                output,
                originalMessage,
                ExtractPercent(output, "CPU load:"),
                ExtractMemoryUsedPercent(output),
                ExtractProcessSummaries(output));
        }

        private static IEnumerable<ProcessSummary> GetSafeCloseCandidates(ToolMemory memory)
        {
            return memory.Processes
                .Where(IsSafeCloseCandidate)
                .OrderByDescending(process => process.MemoryMb);
        }

        private static bool IsSafeCloseCandidate(ProcessSummary process)
        {
            return !IsSystemProcess(process.Name);
        }

        private static List<ProcessSummary> ExtractProcessSummaries(string text)
        {
            var processes = new List<ProcessSummary>();
            foreach (var rawLine in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.Trim();
                if (!line.StartsWith("-", StringComparison.Ordinal))
                {
                    continue;
                }

                var match = System.Text.RegularExpressions.Regex.Match(
                    line,
                    @"^-\s*(?<name>.+?)\s*(?:\|\s*PID\s*(?<pid1>\d+)\s*\||\(PID\s*(?<pid2>\d+)\))[:\s|]+(?<amount>\d+(?:\.\d+)?)\s*(?<unit>GB|MB)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    continue;
                }

                var amount = double.TryParse(match.Groups["amount"].Value, out var parsedAmount)
                    ? parsedAmount
                    : 0;
                var unit = match.Groups["unit"].Value.ToUpperInvariant();
                var memoryMb = unit == "GB" ? amount * 1024 : amount;
                var pidText = match.Groups["pid1"].Success
                    ? match.Groups["pid1"].Value
                    : match.Groups["pid2"].Value;
                int? pid = int.TryParse(pidText, out var parsedPid) ? parsedPid : null;

                processes.Add(new ProcessSummary(
                    match.Groups["name"].Value.Trim(),
                    $"{amount:0.##} {unit}",
                    pid,
                    memoryMb));
            }

            return processes;
        }

        private static bool IsSystemProcess(string name)
        {
            return ContainsAny(name,
                "memory compression",
                "vmmem",
                "windows",
                "defender",
                "msmpeng",
                "system",
                "registry",
                "svchost",
                "service host",
                "dwm",
                "explorer");
        }

        private static bool IsShortContextualFollowUp(string normalized)
        {
            return normalized is "why" or "why?" or "how come" or "how come?" or "explain" or "explain that" ||
                   ContainsAny(normalized,
                       "why is that",
                       "what does that mean",
                       "why does that matter",
                       "what should i do",
                       "what should i close",
                       "what can i close",
                       "what do i close",
                       "what should i stop",
                       "what can i stop",
                       "close the biggest",
                       "stop the biggest",
                       "close biggest",
                       "stop biggest",
                       "close that",
                       "stop that",
                       "is that normal",
                       "is this normal");
        }

        private static bool IsCloseRecommendationRequest(string normalized)
        {
            return ContainsAny(normalized,
                "what should i close",
                "what can i close",
                "what do i close",
                "what should i stop",
                "what can i stop",
                "what should i do");
        }

        private static bool IsCloseBiggestRequest(string normalized)
        {
            return ContainsAny(normalized,
                "close the biggest",
                "stop the biggest",
                "close biggest",
                "stop biggest",
                "close that",
                "stop that",
                "close it",
                "stop it");
        }

        private sealed record ToolMemory(
            string ToolName,
            string Output,
            string OriginalMessage,
            double? CpuPercent,
            double? MemoryUsedPercent,
            List<ProcessSummary> Processes);

        private sealed record ProcessSummary(string Name, string Memory, int? Pid, double MemoryMb);

        private static double? ExtractMemoryUsedPercent(string text)
        {
            var line = text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(value => value.TrimStart().StartsWith("Memory:", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var match = System.Text.RegularExpressions.Regex.Match(line, @"\((?<percent>\d+(?:\.\d+)?)%\s+used\)");
            if (!match.Success)
            {
                return null;
            }

            return double.TryParse(match.Groups["percent"].Value, out var value) ? value : null;
        }

        private static string BuildDefenderSummary(string output)
        {
            if (output.Contains("Antivirus product:", StringComparison.OrdinalIgnoreCase))
            {
                return "Windows reports an antivirus product is registered. Details are below.";
            }

            if (output.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return "I could not confirm Microsoft Defender from the command-line scanner path. Details are below.";
            }

            return "Here is the current Windows security status I could read.";
        }

        private static double? ExtractPercent(string text, string label)
        {
            var line = text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(value => value.TrimStart().StartsWith(label, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var percentIndex = line.IndexOf('%');
            if (percentIndex < 0)
            {
                return null;
            }

            var start = percentIndex - 1;
            while (start >= 0 && (char.IsDigit(line[start]) || line[start] == '.'))
            {
                start--;
            }

            var raw = line.Substring(start + 1, percentIndex - start - 1);
            return double.TryParse(raw, out var value) ? value : null;
        }

        private static string? ExtractToolArg(Dictionary<string, object?>? args, string key)
        {
            if (args == null || !args.TryGetValue(key, out var value))
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(value?.ToString()) ? null : value.ToString();
        }

        private async Task GetAIResponseAsync(string userMessage)
        {
            try
            {
                // Stream tokens into the UI for a live typing effect.
                var response = await _llmService.GetStreamingResponseAsync(
                    userMessage,
                    OnTokenReceived,
                    _conversationId);

                // Post-process final response (e.g., tool confirmations).
                var finalResponse = HandleAssistantPostProcess(response);
                PersistAssistantMessage(finalResponse);
            }
            catch (Exception ex)
            {
                AddMessageToChat($"I could not generate a response: {ex.Message}", "system");
            }
        }

        private Border? _currentStreamingMessage;
        private TextBlock? _currentStreamingTextBlock;

        private void OnTokenReceived(string token)
        {
            Dispatcher.Invoke(() =>
            {
                if (_currentStreamingMessage == null)
                {
                    // First token: create the assistant bubble.
                    _currentStreamingTextBlock = new TextBlock
                    {
                        Foreground = GetChatAssistantForeground(),
                        FontSize = 14,
                        TextWrapping = TextWrapping.Wrap,
                        Text = token
                    };

                    _currentStreamingMessage = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.Assistant"),
                        Child = _currentStreamingTextBlock
                    };

                    ChatMessagesPanel.Children.Add(_currentStreamingMessage);
                }
                else
                {
                    // Append tokens to the current assistant bubble.
                    if (_currentStreamingTextBlock != null)
                    {
                        _currentStreamingTextBlock.Text += token;
                    }
                }

                // Keep the newest content in view.
                ChatScrollViewer.ScrollToEnd();
            });
        }

        private string HandleAssistantPostProcess(string response)
        {
            var finalResponse = response;
            if (ToolConfirmationParser.TryExtractConfirmation(response, out var cleaned, out var call) && call != null)
            {
                finalResponse = cleaned;
                if (_currentStreamingTextBlock != null)
                {
                    _currentStreamingTextBlock.Text = FormatChatDisplayText(finalResponse);
                }

                ShowToolConfirmation(call);
                ChatScrollViewer.ScrollToEnd();
            }

            var displayResponse = FormatChatDisplayText(finalResponse);
            if (_currentStreamingTextBlock != null && _currentStreamingTextBlock.Text != displayResponse)
            {
                _currentStreamingTextBlock.Text = displayResponse;
                ChatScrollViewer.ScrollToEnd();
            }
            else if (_currentStreamingTextBlock == null && !string.IsNullOrWhiteSpace(finalResponse))
            {
                ChatMessagesPanel.Children.Add(CreateMessageBubble(finalResponse, "assistant"));
                ChatScrollViewer.ScrollToEnd();
                return finalResponse;
            }

            return finalResponse;
        }

        private void AddMessageToChat(string message, string role)
        {
            if (ChatMessagesPanel == null || ChatScrollViewer == null)
            {
                return;
            }

            if (role.Equals("assistant", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(message))
            {
                _lastAssistantMessage = message;
            }

            if (!_isReplayingHistory)
            {
                if (role.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    // Only start a session after the first user message.
                    _hasUserMessages = true;
                    _historyService.AppendMessage(_conversationId, role, message);
                }
                else if (_hasUserMessages)
                {
                    _historyService.AppendMessage(_conversationId, role, message);
                }
            }

            ChatMessagesPanel.Children.Add(CreateMessageBubble(message, role));

            // Reset streaming state when a full message is added.
            if (role != "assistant" || _currentStreamingMessage == null)
            {
                _currentStreamingMessage = null;
                _currentStreamingTextBlock = null;
            }

            // Auto-scroll to bottom.
            ChatScrollViewer.ScrollToEnd();
        }

        private void PersistAssistantMessage(string message)
        {
            if (_isReplayingHistory || !_hasUserMessages || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _historyService.AppendMessage(_conversationId, "assistant", message);
            _lastAssistantMessage = message;
            _currentStreamingMessage = null;
            _currentStreamingTextBlock = null;
        }

        private void ShowTypingIndicator()
        {
            TypingIndicator.Visibility = Visibility.Visible;
            _typingTimer.Start();
        }

        private void HideTypingIndicator()
        {
            TypingIndicator.Visibility = Visibility.Collapsed;
            _typingTimer.Stop();
        }

        private void TypingTimer_Tick(object? sender, EventArgs e)
        {
            var currentText = TypingDots.Text;
            TypingDots.Text = currentText.Length >= 6 ? "." : currentText + ".";
        }

        private async void ProviderRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshProviderStatusAsync();
        }

        private async Task RefreshProviderStatusAsync()
        {
            var checkId = ++_providerStatusCheckId;
            SetProviderStatus(
                GetString("Chat.ProviderChecking", "Checking..."),
                GetString("Chat.ProviderCheckingTooltip", "Checking the configured AI Server."),
                GetStatusBrush("Brush.Warning"));

            var prefs = new PreferencesService().Load();
            var baseUrl = NormalizeBackendUrl(prefs.BackendBaseUrl);
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                SetProviderStatus(
                    GetString("Chat.ProviderLocal", "Local mode"),
                    GetString("Chat.ProviderLocalTooltip", "No AI Server is configured. Broad answers use the built-in local assistant."),
                    GetStatusBrush("Brush.Warning"));
                return;
            }

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl.TrimEnd('/')}/health");
                var apiKey = prefs.BackendApiKey?.Trim();
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    request.Headers.Add("X-API-Key", apiKey);
                }

                using var response = await client.SendAsync(request);
                if (checkId != _providerStatusCheckId)
                {
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    SetProviderStatus(
                        GetString("Chat.ProviderServerConnected", "AI Server connected"),
                        Format("Chat.ProviderServerConnectedTooltip", "Connected to {0}. Broad answers can use the AI Server.", baseUrl),
                        GetStatusBrush("Brush.Success"));
                }
                else
                {
                    SetProviderStatus(
                        GetString("Chat.ProviderServerOffline", "AI Server offline, using local"),
                        Format("Chat.ProviderServerFailedTooltip", "The configured AI Server returned HTTP {0}. Local tools still work.", (int)response.StatusCode),
                        GetStatusBrush("Brush.Error"));
                }
            }
            catch
            {
                if (checkId != _providerStatusCheckId)
                {
                    return;
                }

                SetProviderStatus(
                    GetString("Chat.ProviderServerOffline", "AI Server offline, using local"),
                    Format("Chat.ProviderServerOfflineTooltip", "Could not reach {0}. Local tools still work.", baseUrl),
                    GetStatusBrush("Brush.Error"));
            }
        }

        private void SetProviderStatus(string text, string tooltip, Brush indicatorBrush)
        {
            if (ProviderStatusText != null)
            {
                ProviderStatusText.Text = text;
            }

            if (ProviderStatusPill != null)
            {
                ProviderStatusPill.ToolTip = tooltip;
            }

            if (ProviderStatusDot != null)
            {
                ProviderStatusDot.Fill = indicatorBrush;
            }
        }

        private Brush GetStatusBrush(string resourceKey)
        {
            return TryFindResource(resourceKey) as Brush ?? GetChatSystemForeground();
        }

        private static string NormalizeBackendUrl(string? value)
        {
            var trimmed = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = "http://" + trimmed;
            }

            return trimmed.TrimEnd('/');
        }

        private async void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            // Silent clear for cleaner UX
            ChatMessagesPanel.Children.Clear();

            // Add welcome message back
            var welcomeBorder = new Border
            {
                Style = (Style)FindResource("Chat.Bubble.System"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = FormatChatDisplayText(GetString("Chat.Welcome", "Welcome to Smartitecture AI Assistant! I can help you with automation, system commands, and more.")),
                    Style = (Style)FindResource("Text.Caption"),
                    Foreground = GetChatSystemForeground(),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            };
            ChatMessagesPanel.Children.Add(welcomeBorder);
            // Clear conversation history
            if (_llmService != null)
            {
                await _llmService.ClearConversationAsync(_conversationId);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHistoryList();
            LoadDeletedList();
            SetHistoryTab(false);
            ShowHistoryOverlay();
        }

        private void HistoryClose_Click(object sender, RoutedEventArgs e)
        {
            HideHistoryOverlay();
        }

        private void HistoryOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_previewingDeleted)
            {
                return;
            }

            if (HistoryList.SelectedItem is not HistorySessionItem selected)
            {
                return;
            }

            var session = _historyService.GetSession(selected.Id);
            if (session == null)
            {
                return;
            }

            _isReplayingHistory = true;
            ChatMessagesPanel.Children.Clear();
            foreach (var msg in session.Messages)
            {
                ChatMessagesPanel.Children.Add(CreateMessageBubble(msg.Content, msg.Role));
            }
            _isReplayingHistory = false;

            _conversationId = session.Id;
            _hasUserMessages = session.Messages.Any(m => m.Role.Equals("user", StringComparison.OrdinalIgnoreCase));
            HideHistoryOverlay();
            ChatScrollViewer.ScrollToEnd();
        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListContainer != null && HistoryListContainer.Visibility != Visibility.Visible)
            {
                return;
            }

            if (HistoryList.SelectedItem is not HistorySessionItem selected)
            {
                HistoryPreviewPanel.Children.Clear();
                return;
            }

            var session = _historyService.GetSession(selected.Id);
            _previewingDeleted = false;
            if (DeletedList != null)
            {
                DeletedList.SelectedItem = null;
            }
            if (HistoryOpenButton != null)
            {
                HistoryOpenButton.IsEnabled = true;
            }
            RenderHistoryPreview(session);
        }

        private void DeletedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeletedListContainer != null && DeletedListContainer.Visibility != Visibility.Visible)
            {
                return;
            }

            if (DeletedList.SelectedItem is not DeletedSessionItem selected)
            {
                if (_previewingDeleted)
                {
                    HistoryPreviewPanel.Children.Clear();
                }
                return;
            }

            _previewingDeleted = true;
            if (HistoryList != null)
            {
                HistoryList.SelectedItem = null;
            }
            if (HistoryOpenButton != null)
            {
                HistoryOpenButton.IsEnabled = false;
            }

            var session = _historyService.GetDeletedSession(selected.Id);
            RenderHistoryPreview(session);
        }

        private void HistoryTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryTab(false);
        }

        private void DeletedTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryTab(true);
        }

        private void SetHistoryTab(bool showDeleted)
        {
            // Toggle between active chat history and deleted chats.
            _previewingDeleted = showDeleted;

            if (HistoryListContainer != null)
            {
                HistoryListContainer.Visibility = showDeleted ? Visibility.Collapsed : Visibility.Visible;
            }

            if (DeletedListContainer != null)
            {
                DeletedListContainer.Visibility = showDeleted ? Visibility.Visible : Visibility.Collapsed;
            }

            if (HistoryList != null)
            {
                HistoryList.SelectedItem = null;
            }

            if (DeletedList != null)
            {
                DeletedList.SelectedItem = null;
            }

            HistoryPreviewPanel.Children.Clear();

            if (HistoryOpenButton != null)
            {
                HistoryOpenButton.IsEnabled = !showDeleted;
            }

            var primary = TryFindResource("Button.Primary") as Style;
            var glass = TryFindResource("Button.Glass") as Style;
            if (HistoryTabButton != null && primary != null && glass != null)
            {
                HistoryTabButton.Style = showDeleted ? glass : primary;
            }
            if (DeletedTabButton != null && primary != null && glass != null)
            {
                DeletedTabButton.Style = showDeleted ? primary : glass;
            }
        }

        private void RenderHistoryPreview(ChatHistorySession? session)
        {
            // Render a read-only preview of a chat session.
            HistoryPreviewPanel.Children.Clear();
            if (session == null || session.Messages.Count == 0)
            {
                HistoryPreviewPanel.Children.Add(new TextBlock
                {
                    Text = GetString("Chat.HistoryEmpty", "No saved chats yet."),
                    Style = (Style)FindResource("Text.Subtle")
                });
                return;
            }

            foreach (var msg in session.Messages)
            {
                HistoryPreviewPanel.Children.Add(CreateMessageBubble(msg.Content, msg.Role));
            }
        }

        private void LoadHistoryList()
        {
            // Active sessions list.
            var sessions = _historyService.GetSessions();
            var items = sessions.Select(s => new HistorySessionItem
            {
                Id = s.Id,
                Title = string.IsNullOrWhiteSpace(s.Title) || s.Title == "New Chat"
                    ? GetString("Chat.HistoryUntitled", "New Chat")
                    : s.Title,
                Time = s.LastUpdated.ToString("g")
            }).ToList();

            HistoryList.ItemsSource = items;
            HistoryEmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            HistoryPreviewPanel.Children.Clear();
        }

        private void LoadDeletedList()
        {
            // Deleted sessions list (recoverable).
            var sessions = _historyService.GetDeletedSessions();
            var items = sessions.Select(s => new DeletedSessionItem
            {
                Id = s.Id,
                Title = string.IsNullOrWhiteSpace(s.Title) || s.Title == "New Chat"
                    ? GetString("Chat.HistoryUntitled", "New Chat")
                    : s.Title,
                Time = (s.DeletedAt ?? s.LastUpdated).ToString("g")
            }).ToList();

            if (DeletedList != null)
            {
                DeletedList.ItemsSource = items;
            }

            if (DeletedEmptyText != null)
            {
                DeletedEmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ShowHistoryOverlay()
        {
            // Slide-in side panel (no layout shift).
            if (HistoryPanel == null)
            {
                return;
            }

            HistoryPanel.Opacity = 0;
            HistoryPanel.Visibility = Visibility.Visible;

            var panelTransform = HistoryPanel.RenderTransform as TranslateTransform ?? new TranslateTransform(40, 0);
            HistoryPanel.RenderTransform = panelTransform;

            var panelFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
            var panelSlide = new DoubleAnimation(panelTransform.X, 0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            HistoryPanel.BeginAnimation(OpacityProperty, panelFade);
            panelTransform.BeginAnimation(TranslateTransform.XProperty, panelSlide);
        }

        private void HideHistoryOverlay()
        {
            // Slide-out side panel.
            if (HistoryPanel == null)
            {
                return;
            }

            var panelTransform = HistoryPanel.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
            HistoryPanel.RenderTransform = panelTransform;

            var panelSlide = new DoubleAnimation(panelTransform.X, 40, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var panelFade = new DoubleAnimation(HistoryPanel.Opacity, 0, TimeSpan.FromMilliseconds(160));
            panelFade.Completed += (_, __) =>
            {
                HistoryPanel.Visibility = Visibility.Collapsed;
                HideDeleteConfirm();
                _previewingDeleted = false;
                if (HistoryOpenButton != null)
                {
                    HistoryOpenButton.IsEnabled = true;
                }
            };

            panelTransform.BeginAnimation(TranslateTransform.XProperty, panelSlide);
            HistoryPanel.BeginAnimation(OpacityProperty, panelFade);
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            // Soft-delete: move to Deleted tab.
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            ShowDeleteConfirm(DeleteConfirmMode.Soft, id);
        }

        private void RecoverDeleted_Click(object sender, RoutedEventArgs e)
        {
            // Restore a deleted session.
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            _historyService.Restore(id);
            LoadHistoryList();
            LoadDeletedList();

            if (_previewingDeleted)
            {
                _previewingDeleted = false;
                HistoryPreviewPanel.Children.Clear();
            }
        }

        private void DeleteForever_Click(object sender, RoutedEventArgs e)
        {
            // Permanent delete requires confirmation.
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            ShowDeleteConfirm(DeleteConfirmMode.Permanent, id);
        }

        private void DeleteCancel_Click(object sender, RoutedEventArgs e)
        {
            _pendingDeleteId = null;
            _deleteConfirmMode = DeleteConfirmMode.Soft;
            HideDeleteConfirm();
        }

        private void DeleteConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Apply the delete decision (soft or permanent).
            if (string.IsNullOrWhiteSpace(_pendingDeleteId))
            {
                HideDeleteConfirm();
                return;
            }

            if (_deleteConfirmMode == DeleteConfirmMode.Permanent)
            {
                _historyService.DeletePermanently(_pendingDeleteId);
            }
            else
            {
                _historyService.MarkDeleted(_pendingDeleteId);
            }

            if (_pendingDeleteId == _conversationId)
            {
                _conversationId = Guid.NewGuid().ToString();
                _hasUserMessages = false;
                ResetChatToWelcome();
            }

            _pendingDeleteId = null;
            _deleteConfirmMode = DeleteConfirmMode.Soft;
            LoadHistoryList();
            LoadDeletedList();
            HistoryPreviewPanel.Children.Clear();
            HideDeleteConfirm();
        }

        private void ShowDeleteConfirm(DeleteConfirmMode mode, string sessionId)
        {
            // Inline confirmation inside the preview panel.
            if (DeleteConfirmCard == null)
            {
                return;
            }

            _pendingDeleteId = sessionId;
            _deleteConfirmMode = mode;

            if (DeleteConfirmTitleText != null)
            {
                DeleteConfirmTitleText.Text = mode == DeleteConfirmMode.Permanent
                    ? GetString("Chat.DeleteForeverTitle", "Delete permanently?")
                    : GetString("Chat.DeleteTitle", "Delete this chat?");
            }

            if (DeleteConfirmWarningText != null)
            {
                DeleteConfirmWarningText.Text = mode == DeleteConfirmMode.Permanent
                    ? GetString("Chat.DeleteForeverWarning", "This permanently removes the chat and cannot be undone.")
                    : GetString("Chat.DeleteWarning", "Warning: you are about to delete this chat.");
            }

            if (DeleteConfirmRecoveryText != null)
            {
                if (mode == DeleteConfirmMode.Permanent)
                {
                    DeleteConfirmRecoveryText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DeleteConfirmRecoveryText.Visibility = Visibility.Visible;
                    DeleteConfirmRecoveryText.Text = GetString("Chat.DeleteRecovery", "Once you delete, you have 30 days to recover it before it is removed automatically.");
                }
            }

            DeleteConfirmCard.Visibility = Visibility.Visible;
            DeleteConfirmCard.Opacity = 0;
            var scale = DeleteConfirmCard.RenderTransform as ScaleTransform ?? new ScaleTransform(0.96, 0.96);
            DeleteConfirmCard.RenderTransform = scale;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(140));
            var scaleAnim = new DoubleAnimation(0.96, 1, TimeSpan.FromMilliseconds(160))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DeleteConfirmCard.BeginAnimation(OpacityProperty, fade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void HideDeleteConfirm()
        {
            // Animate confirmation card away.
            if (DeleteConfirmCard == null || DeleteConfirmCard.Visibility != Visibility.Visible)
            {
                return;
            }

            var scale = DeleteConfirmCard.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
            DeleteConfirmCard.RenderTransform = scale;

            var fade = new DoubleAnimation(DeleteConfirmCard.Opacity, 0, TimeSpan.FromMilliseconds(120));
            var scaleAnim = new DoubleAnimation(scale.ScaleX, 0.96, TimeSpan.FromMilliseconds(120))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fade.Completed += (_, __) =>
            {
                DeleteConfirmCard.Visibility = Visibility.Collapsed;
            };

            DeleteConfirmCard.BeginAnimation(OpacityProperty, fade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void ShowToolConfirmation(ToolCall call)
        {
            // Full-screen overlay asking for user confirmation.
            _pendingToolConfirmation = call;
            if (ToolConfirmOverlay == null || ToolConfirmCard == null)
            {
                return;
            }

            if (ToolConfirmMessage != null)
            {
                ToolConfirmMessage.Text = Format(
                    "Chat.ToolConfirmMessage",
                    "You\u2019re about to run {0}. Continue?",
                    call.Name);
            }

            ToolConfirmOverlay.Visibility = Visibility.Visible;
            ToolConfirmOverlay.Opacity = 0;

            var scale = ToolConfirmCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(0.96, 0.96);
                ToolConfirmCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(160));
            var scaleIn = new DoubleAnimation(0.96, 1.0, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            ToolConfirmOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);
        }

        private void HideToolConfirmation()
        {
            // Fade out the confirmation overlay.
            if (ToolConfirmOverlay == null || ToolConfirmCard == null)
            {
                return;
            }

            var scale = ToolConfirmCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(1, 1);
                ToolConfirmCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(ToolConfirmOverlay.Opacity, 0, TimeSpan.FromMilliseconds(140));
            overlayFade.Completed += (_, __) => ToolConfirmOverlay.Visibility = Visibility.Collapsed;
            var scaleOut = new DoubleAnimation(scale.ScaleX, 0.96, TimeSpan.FromMilliseconds(140))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            ToolConfirmOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
        }

        private void ToolConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            // User cancels the tool action.
            _pendingToolConfirmation = null;
            HideToolConfirmation();
        }

        private async void ToolConfirmConfirm_Click(object sender, RoutedEventArgs e)
        {
            // User approves the tool action.
            if (_pendingToolConfirmation == null)
            {
                HideToolConfirmation();
                return;
            }

            var call = _pendingToolConfirmation;
            _pendingToolConfirmation = null;
            HideToolConfirmation();

            var result = await _toolExecutor.ExecuteToolAsync(call.Name, call.ArgumentsJson, true);
            AddMessageToChat(result.Message, "system");
        }

        private void ResetChatToWelcome()
        {
            // Reset chat UI when there is no active session.
            _hasUserMessages = false;
            ChatMessagesPanel.Children.Clear();
            ChatMessagesPanel.Children.Add(new Border
            {
                Style = (Style)FindResource("Chat.Bubble.System"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = GetString("Chat.Welcome", "Welcome to Smartitecture AI Assistant! I can help you with automation, system commands, and more."),
                    Style = (Style)FindResource("Text.Caption"),
                    Foreground = GetChatSystemForeground(),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            });
            ChatScrollViewer.ScrollToEnd();
        }

        private Border CreateMessageBubble(string message, string role)
        {
            // Builds a chat bubble based on role (user/assistant/system).
            var textBlock = new TextBlock
            {
                Text = FormatChatDisplayText(message),
                Foreground = GetChatAssistantForeground(),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            Border messageBorder;
            switch (role.ToLower())
            {
                case "user":
                    messageBorder = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.User"),
                        Child = textBlock
                    };
                    textBlock.Foreground = GetChatUserForeground();
                    break;
                case "system":
                    messageBorder = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.System"),
                        Child = textBlock
                    };
                    textBlock.Foreground = GetChatSystemForeground();
                    textBlock.FontSize = 12;
                    break;
                default:
                    messageBorder = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.Assistant"),
                        Child = textBlock
                    };
                    textBlock.Foreground = GetChatAssistantForeground();
                    break;
            }

            return messageBorder;
        }

        private static string FormatChatDisplayText(string message)
        {
            return ResponseTextCleaner.ForChatDisplay(message);
        }

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = GetString("Chat.Attach", "Attach"),
                Multiselect = true,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true || dialog.FileNames.Length == 0)
            {
                return;
            }

            var attachmentText = string.Join(Environment.NewLine, dialog.FileNames.Select(path => $"Attached file: {path}"));
            if (MessageInput.Text == GetString("Chat.Placeholder", "Type your message or command here..."))
            {
                MessageInput.Text = string.Empty;
                MessageInput.Foreground = GetOnSurfaceBrush();
            }

            MessageInput.Text = string.IsNullOrWhiteSpace(MessageInput.Text)
                ? attachmentText
                : $"{MessageInput.Text}{Environment.NewLine}{attachmentText}";
            MessageInput.CaretIndex = MessageInput.Text.Length;
            MessageInput.Focus();
        }

        private async void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessing)
            {
                return;
            }

            var originalContent = VoiceButton.Content;
            VoiceButton.IsEnabled = false;
            VoiceButton.Content = GetString("Chat.Listening", "Listening...");

            try
            {
                var result = await _voiceInput.ListenOnceAsync();
                if (!result.Success || string.IsNullOrWhiteSpace(result.Text))
                {
                    AddMessageToChat(result.Message, "system");
                    return;
                }

                MessageInput.Text = result.Text.Trim();
                MessageInput.Foreground = GetOnSurfaceBrush();
                await SendMessageAsync();
            }
            catch (Exception ex)
            {
                AddMessageToChat($"Voice input error: {ex.Message}", "system");
            }
            finally
            {
                VoiceButton.Content = originalContent;
                VoiceButton.IsEnabled = true;
                MessageInput.Focus();
            }
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            // Top bar navigation.
            Smartitecture.Services.NavigationService.GoHome();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            // Top bar navigation.
            Smartitecture.Services.NavigationService.GoSettings();
        }

        private class HistorySessionItem
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Time { get; set; } = string.Empty;
        }

        private class DeletedSessionItem
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Time { get; set; } = string.Empty;
        }

        private static string GetString(string key, string fallback)
        {
            // Localization helper.
            return Application.Current?.TryFindResource(key) as string ?? fallback;
        }

        private static string Format(string key, string fallback, params object[] args)
        {
            // Localization + formatting helper.
            var template = GetString(key, fallback);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return string.Format(fallback, args);
            }
        }
    }
}
