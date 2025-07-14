using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smartitecture.Core.Services.Base;

namespace Smartitecture.Core.Services
{
    public class PythonApiService : BaseService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PythonApiService> _logger;
        private readonly string _baseUrl;

        public PythonApiService(HttpClient httpClient, ILogger<PythonApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = "http://127.0.0.1:8000";
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check Python API health");
                return false;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to GET from Python API: {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to POST to Python API: {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<string> GetConfigurationAsync(string key)
        {
            var response = await GetAsync<dynamic>($"/api/config/{key}");
            return response?.value?.ToString();
        }

        public async Task SetConfigurationAsync(string key, string value)
        {
            var data = new { key, value };
            await PostAsync<dynamic>("/api/config", data);
        }

        public async Task<string> ProcessTextAsync(string inputText, string model = "gpt-4")
        {
            var data = new { input_text = inputText, model };
            var response = await PostAsync<dynamic>("/api/process", data);
            return response?.output_text?.ToString();
        }

        public async Task<string> ReadFileAsync(string path)
        {
            var data = new { path };
            var response = await PostAsync<dynamic>("/api/file/read", data);
            return response?.content?.ToString();
        }

        public async Task WriteFileAsync(string path, string content)
        {
            var data = new { path, content };
            await PostAsync<dynamic>("/api/file/write", data);
        }

        public async Task<bool> DeleteFileAsync(string path)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/file/{path}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {Path}", path);
                return false;
            }
        }

        protected override async Task<bool> ExecuteAsync()
        {
            // Check if Python API is running
            return await IsHealthyAsync();
        }
    }
}
