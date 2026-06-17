using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Smartitecture.Services
{
    /// <summary>
    /// Production local assistant used when no configured LLM provider is available.
    /// It gives deterministic, useful answers and is explicit about its offline limits.
    /// </summary>
    public sealed class NaturalConversationService
    {
        private readonly KnowledgeBaseService _knowledgeBase = new();

        public Task<string> GetResponseAsync(string message, List<ConversationMessage>? history = null)
        {
            var normalized = Normalize(message);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return Task.FromResult("Type a question or a command. I can check this PC, open apps, calculate, explain basic concepts, and help troubleshoot common Windows issues.");
            }

            var math = TryAnswerMath(message);
            if (!string.IsNullOrWhiteSpace(math))
            {
                return Task.FromResult(math);
            }

            var knowledgeAnswer = _knowledgeBase.GetAnswer(message);
            if (!string.IsNullOrWhiteSpace(knowledgeAnswer))
            {
                return Task.FromResult(knowledgeAnswer);
            }

            return Task.FromResult(GenerateResponse(message, normalized, history));
        }

        private static string GenerateResponse(string original, string normalized, List<ConversationMessage>? history)
        {
            if (IsGreeting(normalized))
            {
                return "Hi. I can help with this PC, open apps, run safe system checks, do calculations, and answer basic questions. Try: \"check my PC\", \"why is my PC slow\", \"show running processes\", or \"open calculator\".";
            }

            if (ContainsAny(normalized, "thank", "thanks", "appreciate it"))
            {
                return "You're welcome. What do you want to check or do next?";
            }

            if (ContainsAny(normalized, "what can you do", "help", "commands", "capabilities", "how do i use you", "what do you support"))
            {
                return BuildCapabilitiesMessage();
            }

            var contextualFollowUp = TryAnswerContextualFollowUp(normalized, history);
            if (!string.IsNullOrWhiteSpace(contextualFollowUp))
            {
                return contextualFollowUp;
            }

            if (normalized.StartsWith("i just executed the ", StringComparison.OrdinalIgnoreCase))
            {
                return "Done. The command ran successfully.";
            }

            if (normalized.Contains(" command failed", StringComparison.OrdinalIgnoreCase))
            {
                return "That command did not complete. Check that the app or Windows feature exists on this PC, then try a more specific command such as \"open calculator\", \"open task manager\", or \"check performance\".";
            }

            if (ContainsAny(normalized, "are you online", "internet", "web", "search the web", "latest", "current news"))
            {
                return "I can use local PC information right now. Live web research needs a configured cloud or backend provider. For local work, ask me to check PC info, performance, Defender status, network adapters, or running processes.";
            }

            if (ContainsAny(normalized, "backend", "ai server", "server ai", "connect to server", "cloud provider", "on demand"))
            {
                return BuildBackendSetupMessage();
            }

            if (ContainsAny(normalized, "privacy", "data", "what do you collect", "safe"))
            {
                return "Local commands run on this computer and are logged for safety. Destructive actions, like stopping a process or starting a Defender scan, ask for confirmation first. Cloud AI only works when you configure a provider.";
            }

            if (ContainsAny(normalized, "voice", "microphone", "mic", "speech"))
            {
                return "Voice input uses the computer microphone through Windows speech recognition. If it does not work, enable Windows Settings > Privacy & security > Speech > Online speech recognition, then allow microphone access for desktop apps.";
            }

            if (ContainsAny(normalized, "slow", "lag", "laggy", "freezing", "stutter", "performance"))
            {
                return "A slow PC is usually caused by one of five things: high CPU load, high memory pressure, heavy disk activity, too many startup/background apps, or heat causing throttling. Ask \"why is my PC slow\" or \"check performance\" and I will read the live signals I can access, then point to the most likely cause.";
            }

            if (ContainsAny(normalized, "hot", "overheat", "overheating", "fan", "temperature"))
            {
                return "A hot PC is usually caused by sustained CPU/GPU work, blocked airflow, dust, a soft surface, aggressive charging, or thermal throttling. I can read CPU, memory, and top processes now. Ask \"why is my PC hot\" and I will explain what the local signals show; direct temperature sensors are not available in this build.";
            }

            if (ContainsAny(normalized, "virus", "malware", "defender", "antivirus", "security"))
            {
                return "I can check Windows security status or start a Defender scan. Try \"Defender status\" or \"scan my PC\". A scan asks for confirmation before it starts.";
            }

            if (ContainsAny(normalized, "network status", "network adapter", "network adapters", "wifi", "wi-fi", "internet not working", "ip address", "show my ip", "what is my ip", "what's my ip"))
            {
                return "I can inspect the network adapters and IP addresses this PC reports. Type \"network status\" or \"show my IP address\".";
            }

            if (ContainsAny(normalized, "open", "launch", "start", "run"))
            {
                return "Tell me the app name directly, for example: \"open calculator\", \"open notepad\", \"open file explorer\", \"open settings\", or \"open task manager\".";
            }

            if (ContainsAny(normalized, "file", "folder", "organize", "download"))
            {
                return "File automation is limited in this build. I can open File Explorer now. For production file actions, the next safe step is adding explicit confirmation for create, move, rename, and delete operations.";
            }

            if (ContainsAny(normalized, "who are you", "what are you", "about you"))
            {
                return "I'm Smartitecture Assistant, a Windows-focused helper for local PC checks, app launching, safe system actions, calculations, and basic explanations. If a cloud or local LLM provider is configured, I can handle broader natural-language requests too.";
            }

            var commonExplanation = TryAnswerCommonExplanation(normalized);
            if (!string.IsNullOrWhiteSpace(commonExplanation))
            {
                return commonExplanation;
            }

            if (LooksLikeQuestion(normalized))
            {
                return BuildUnknownButUsefulResponse(normalized);
            }

            return "I’m ready. Ask a question or give a PC command like \"check my PC\", \"check performance\", \"show processes\", \"network status\", \"Defender status\", or \"open calculator\".";
        }

        private static string BuildCapabilitiesMessage()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Here is what I can do in this build:",
                "",
                "- Check this PC: \"check my PC\" or \"system info\".",
                "- Diagnose slowness: \"check performance\" or \"show running processes\".",
                "- Network info: \"network status\" or \"show my IP address\".",
                "- Security: \"Defender status\" or \"scan my PC\".",
                "- Open apps: \"open calculator\", \"open settings\", \"open task manager\", \"open file explorer\".",
                "- Basic questions and calculations: \"25 * 18\", \"what time is it\", \"what is AI\".",
                "",
                "Sensitive actions ask for confirmation first. Broader open-ended AI answers use the Smartitecture backend when it is configured; otherwise I stay in local assistant mode."
            });
        }

        private static string BuildUnknownButUsefulResponse()
        {
            return BuildUnknownButUsefulResponse(string.Empty);
        }

        private static string BuildUnknownButUsefulResponse(string normalized)
        {
            if (normalized.StartsWith("why ", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("how ", StringComparison.OrdinalIgnoreCase))
            {
                return string.Join(Environment.NewLine, new[]
                {
                    "I do not have enough verified context to explain that reliably in local mode.",
                    "",
                    "What I can do next:",
                    "- If it is about this PC, ask me to run a local check such as \"check performance\", \"show processes\", \"network status\", or \"Defender status\".",
                    "- If it is a broader knowledge question, connect Settings > AI Server so I can answer through the backend instead of guessing.",
                    "",
                    "I would rather say what I can verify than make up a confident-sounding answer."
                });
            }

            return string.Join(Environment.NewLine, new[]
            {
                "I can’t answer that reliably with the built-in local assistant.",
                "",
                "What I can do right now:",
                "- Run local PC checks and diagnostics.",
                "- Open apps and Windows tools.",
                "- Answer basic verified facts and calculations.",
                "",
                "For broader questions, open Settings > AI Server and connect your Smartitecture backend so I can answer through the server on demand."
            });
        }

        private static string BuildBackendSetupMessage()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Yes. Smartitecture can use a server on demand.",
                "",
                "How it works:",
                "- Local commands stay on this PC for diagnostics, app launch, and safe automation.",
                "- Broad questions go to the configured AI Server when Settings > AI Server has a server URL.",
                "- If the server is offline, the app falls back to the local assistant and says so.",
                "",
                "This gives you production behavior without making every PC carry a large local model."
            });
        }

        private static string? TryAnswerContextualFollowUp(string normalized, List<ConversationMessage>? history)
        {
            if (history == null || history.Count == 0)
            {
                return null;
            }

            var isShortWhy = normalized is "why" or "why?" or "how come" or "how come?";
            var isFollowUp = isShortWhy || ContainsAny(normalized, "explain that", "why is that", "what does that mean");
            if (!isFollowUp)
            {
                return null;
            }

            var recentAssistant = history
                .LastOrDefault(m => string.Equals(m.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                ?.Content
                ?.ToLowerInvariant() ?? string.Empty;

            if (ContainsAny(recentAssistant, "memory", "ram", "processes", "cpu", "performance", "slow"))
            {
                return "That matters because Windows has to share CPU time, memory, disk bandwidth, and thermal headroom across every running app. When one app uses a lot of memory or CPU, other apps wait longer, Windows may compress memory or page to disk, and the whole PC can feel slower. The next useful check is to compare top CPU and memory processes, then close or update anything that looks unusually heavy.";
            }

            if (ContainsAny(recentAssistant, "network", "adapter", "ip address", "wi-fi", "wifi"))
            {
                return "That matters because each network adapter can have a different connection state and IP address. The active adapter is the one Windows is actually using for traffic. If an adapter is down or has a link-local address, it may not have a normal connection to your router or network.";
            }

            if (ContainsAny(recentAssistant, "defender", "security", "antivirus", "scan"))
            {
                return "That matters because Defender status tells us whether Windows has active protection available. If protection is disabled, stale, or blocked, the PC may be more exposed. A scan is useful when you suspect malware, but it can take time and use system resources, so Smartitecture asks before starting it.";
            }

            return null;
        }

        private static string? TryAnswerCommonExplanation(string normalized)
        {
            if (ContainsAny(normalized, "how does ai work", "how ai works", "why does ai work"))
            {
                return "AI systems learn patterns from data, then use those patterns to make predictions or generate useful output. For chatbots, the model reads your message, estimates what response is most likely to be helpful, and produces text one piece at a time. It does not truly understand like a person, so production apps should ground it with tools, trusted data, and safety checks.";
            }

            if (ContainsAny(normalized, "what is ram", "what is memory", "why ram matters", "why memory matters"))
            {
                return "RAM is the fast working space your PC uses for open apps and active data. When RAM gets crowded, Windows may compress memory or move data to disk, which is much slower. That is why too many browser tabs, editors, games, or background apps can make the system feel sluggish.";
            }

            if (ContainsAny(normalized, "what is cpu", "why cpu matters", "processor"))
            {
                return "The CPU is the main processor that runs app code and Windows tasks. High CPU usage means programs are competing for processing time. Short spikes are normal; sustained high usage can make apps lag, raise fan noise, and increase heat.";
            }

            if (ContainsAny(normalized, "why is internet slow", "why is my internet slow", "wifi slow", "wi-fi slow"))
            {
                return "Slow internet can come from weak Wi-Fi signal, router congestion, background downloads, VPN overhead, DNS issues, or the internet provider itself. Ask \"network status\" first so I can show the adapters and link speeds this PC reports.";
            }

            return null;
        }

        private static string? TryAnswerMath(string message)
        {
            var expression = ExtractMathExpression(message);
            if (string.IsNullOrWhiteSpace(expression))
            {
                return null;
            }

            try
            {
                var parser = new MathExpressionParser(expression);
                var result = parser.Parse();
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    return "I could not calculate that because the expression has an invalid operation, such as division by zero.";
                }

                return $"{expression.Trim()} = {FormatNumber(result)}";
            }
            catch
            {
                return null;
            }
        }

        private static string? ExtractMathExpression(string message)
        {
            var cleaned = message
                .Replace("×", "*")
                .Replace("÷", "/")
                .Replace("plus", "+", StringComparison.OrdinalIgnoreCase)
                .Replace("minus", "-", StringComparison.OrdinalIgnoreCase)
                .Replace("times", "*", StringComparison.OrdinalIgnoreCase)
                .Replace("multiplied by", "*", StringComparison.OrdinalIgnoreCase)
                .Replace("divided by", "/", StringComparison.OrdinalIgnoreCase);

            var match = Regex.Match(cleaned, @"[-+*/().\d\s]{3,}");
            if (!match.Success)
            {
                return null;
            }

            var expression = match.Value.Trim();
            if (!Regex.IsMatch(expression, @"\d\s*[-+*/]\s*\d"))
            {
                return null;
            }

            return expression;
        }

        private static string FormatNumber(double value)
        {
            if (Math.Abs(value - Math.Round(value)) < 0.0000000001)
            {
                return Math.Round(value).ToString("N0", CultureInfo.CurrentCulture);
            }

            return value.ToString("N6", CultureInfo.CurrentCulture).TrimEnd('0').TrimEnd('.');
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool IsGreeting(string message)
        {
            return Regex.IsMatch(message, @"^(hi|hello|hey|yo|good morning|good afternoon|good evening)\b");
        }

        private static bool LooksLikeQuestion(string message)
        {
            return message.EndsWith("?") ||
                   Regex.IsMatch(message, @"^(what|why|how|when|where|who|can|could|should|would|is|are|do|does|did)\b");
        }

        private static bool ContainsAny(string value, params string[] terms)
        {
            return terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private sealed class MathExpressionParser
        {
            private readonly string _text;
            private int _pos;

            public MathExpressionParser(string text)
            {
                _text = text;
            }

            public double Parse()
            {
                var value = ParseExpression();
                SkipWhiteSpace();
                if (_pos != _text.Length)
                {
                    throw new FormatException("Unexpected characters in expression.");
                }

                return value;
            }

            private double ParseExpression()
            {
                var value = ParseTerm();
                while (true)
                {
                    SkipWhiteSpace();
                    if (Match('+'))
                    {
                        value += ParseTerm();
                    }
                    else if (Match('-'))
                    {
                        value -= ParseTerm();
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            private double ParseTerm()
            {
                var value = ParseFactor();
                while (true)
                {
                    SkipWhiteSpace();
                    if (Match('*'))
                    {
                        value *= ParseFactor();
                    }
                    else if (Match('/'))
                    {
                        value /= ParseFactor();
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            private double ParseFactor()
            {
                SkipWhiteSpace();
                if (Match('+'))
                {
                    return ParseFactor();
                }

                if (Match('-'))
                {
                    return -ParseFactor();
                }

                if (Match('('))
                {
                    var value = ParseExpression();
                    if (!Match(')'))
                    {
                        throw new FormatException("Missing closing parenthesis.");
                    }

                    return value;
                }

                return ParseNumber();
            }

            private double ParseNumber()
            {
                SkipWhiteSpace();
                var start = _pos;
                while (_pos < _text.Length && (char.IsDigit(_text[_pos]) || _text[_pos] == '.'))
                {
                    _pos++;
                }

                if (start == _pos)
                {
                    throw new FormatException("Expected number.");
                }

                var raw = _text.Substring(start, _pos - start);
                return double.Parse(raw, CultureInfo.InvariantCulture);
            }

            private bool Match(char expected)
            {
                SkipWhiteSpace();
                if (_pos >= _text.Length || _text[_pos] != expected)
                {
                    return false;
                }

                _pos++;
                return true;
            }

            private void SkipWhiteSpace()
            {
                while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos]))
                {
                    _pos++;
                }
            }
        }
    }
}
