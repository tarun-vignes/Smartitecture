using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmartitectureSimple
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private const string DefaultModel = "llama3.1";
        private const string OllamaBaseUrl = "http://localhost:11434";

        public OllamaService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
        }

        public async Task<bool> IsOllamaRunningAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{OllamaBaseUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{OllamaBaseUrl}/api/tags");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    var models = new List<string>();
                    
                    if (data.TryGetProperty("models", out var modelsArray))
                    {
                        foreach (var model in modelsArray.EnumerateArray())
                        {
                            if (model.TryGetProperty("name", out var name))
                            {
                                models.Add(name.GetString() ?? "");
                            }
                        }
                    }
                    return models;
                }
            }
            catch { }
            return new List<string>();
        }

        public async Task<string> GenerateResponseAsync(string prompt, string model = DefaultModel)
        {
            try
            {
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.7,
                        top_p = 0.9,
                        max_tokens = 1000
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{OllamaBaseUrl}/api/generate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);
                    
                    if (responseData.TryGetProperty("response", out var responseText))
                    {
                        return responseText.GetString() ?? "No response generated.";
                    }
                }
                else
                {
                    return $"Error: HTTP {response.StatusCode} - {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                return $"Error connecting to Ollama: {ex.Message}";
            }

            return "Failed to generate response.";
        }

        public async Task<string> GenerateWindowsAutomationCommandAsync(string userInput)
        {
            var systemPrompt = @"You are a Windows automation assistant. Convert natural language requests into specific automation commands.

Available automation capabilities:
- Screenshot capture
- Window management (focus, minimize, maximize, close)
- Process management (start, stop, list)
- File operations (create, delete, copy, move)
- System information (CPU, memory, disk usage)
- Network operations (ping, connectivity check)
- Text input simulation
- Mouse control (click, move)
- Keyboard shortcuts

Respond with a JSON object containing:
{
  ""action"": ""action_type"",
  ""parameters"": {
    ""key"": ""value""
  },
  ""description"": ""Human readable description of what will happen""
}

Examples:
- ""Take a screenshot"" → {""action"": ""screenshot"", ""parameters"": {}, ""description"": ""Capturing desktop screenshot""}
- ""Focus on Chrome"" → {""action"": ""focus_window"", ""parameters"": {""process"": ""chrome""}, ""description"": ""Bringing Chrome window to front""}
- ""Check system performance"" → {""action"": ""system_info"", ""parameters"": {}, ""description"": ""Gathering CPU, memory, and disk usage information""}

User request: " + userInput;

            return await GenerateResponseAsync(systemPrompt);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
