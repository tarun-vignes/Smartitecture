using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Smartitecture.Services.AgentTools
{
    /// <summary>
    /// Provides web-related tools for the AI agent to use.
    /// </summary>
    public static class WebTools
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Creates a tool for fetching web content.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <returns>An agent tool for fetching web content</returns>
        public static AgentTool CreateWebFetchTool(ILogger logger)
        {
            return new AgentTool
            {
                Name = "FetchWebContent",
                Description = "Fetches content from a web URL",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["url"] = new ToolParameter
                    {
                        Name = "url",
                        Description = "The URL to fetch content from",
                        Type = "string",
                        Required = true
                    }
                },
                Execute = async (parameters) =>
                {
                    try
                    {
                        var url = parameters["url"].ToString();
                        logger.LogInformation("Fetching content from URL: {Url}", url);

                        var response = await _httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        
                        var content = await response.Content.ReadAsStringAsync();
                        
                        // Truncate content if it's too large
                        if (content.Length > 10000)
                        {
                            content = content.Substring(0, 10000) + "... [Content truncated]";
                        }

                        return new
                        {
                            Url = url,
                            StatusCode = (int)response.StatusCode,
                            ContentType = response.Content.Headers.ContentType?.ToString(),
                            Content = content
                        };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error fetching web content");
                        return new
                        {
                            Success = false,
                            Error = ex.Message
                        };
                    }
                }
            };
        }

        /// <summary>
        /// Creates a tool for getting weather information.
        /// </summary>
        /// <param name="apiKey">The API key for the weather service</param>
        /// <param name="logger">The logger to use</param>
        /// <returns>An agent tool for getting weather information</returns>
        public static AgentTool CreateWeatherTool(string apiKey, ILogger logger)
        {
            return new AgentTool
            {
                Name = "GetWeatherInfo",
                Description = "Gets current weather information for a location",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["location"] = new ToolParameter
                    {
                        Name = "location",
                        Description = "The location to get weather for (city name or zip code)",
                        Type = "string",
                        Required = true
                    }
                },
                Execute = async (parameters) =>
                {
                    try
                    {
                        var location = parameters["location"].ToString();
                        logger.LogInformation("Getting weather for location: {Location}", location);

                        // Replace with your preferred weather API
                        var url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={apiKey}&units=imperial";
                        
                        var response = await _httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        
                        var content = await response.Content.ReadAsStringAsync();
                        var weatherData = JsonSerializer.Deserialize<JsonElement>(content);
                        
                        return new
                        {
                            Location = location,
                            Temperature = weatherData.GetProperty("main").GetProperty("temp").GetDouble(),
                            FeelsLike = weatherData.GetProperty("main").GetProperty("feels_like").GetDouble(),
                            Description = weatherData.GetProperty("weather")[0].GetProperty("description").GetString(),
                            Humidity = weatherData.GetProperty("main").GetProperty("humidity").GetInt32(),
                            WindSpeed = weatherData.GetProperty("wind").GetProperty("speed").GetDouble()
                        };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error getting weather information");
                        return new
                        {
                            Success = false,
                            Error = ex.Message
                        };
                    }
                }
            };
        }

        /// <summary>
        /// Creates a tool for searching the web.
        /// </summary>
        /// <param name="searchApiKey">The API key for the search service</param>
        /// <param name="searchEngineId">The search engine ID</param>
        /// <param name="logger">The logger to use</param>
        /// <returns>An agent tool for searching the web</returns>
        public static AgentTool CreateWebSearchTool(string searchApiKey, string searchEngineId, ILogger logger)
        {
            return new AgentTool
            {
                Name = "SearchWeb",
                Description = "Searches the web for information",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["query"] = new ToolParameter
                    {
                        Name = "query",
                        Description = "The search query",
                        Type = "string",
                        Required = true
                    },
                    ["resultCount"] = new ToolParameter
                    {
                        Name = "resultCount",
                        Description = "The number of results to return (max 10)",
                        Type = "number",
                        Required = false
                    }
                },
                Execute = async (parameters) =>
                {
                    try
                    {
                        var query = parameters["query"].ToString();
                        var resultCount = parameters.ContainsKey("resultCount") 
                            ? Convert.ToInt32(parameters["resultCount"]) 
                            : 5;
                        
                        // Limit to reasonable number
                        resultCount = Math.Min(resultCount, 10);
                        
                        logger.LogInformation("Searching web for: {Query}", query);

                        // Using Google Custom Search API as an example
                        var url = $"https://www.googleapis.com/customsearch/v1?key={searchApiKey}&cx={searchEngineId}&q={Uri.EscapeDataString(query)}&num={resultCount}";
                        
                        var response = await _httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        
                        var content = await response.Content.ReadAsStringAsync();
                        var searchData = JsonSerializer.Deserialize<JsonElement>(content);
                        
                        // Extract search results
                        var items = searchData.GetProperty("items");
                        var results = new List<object>();
                        
                        for (int i = 0; i < items.GetArrayLength(); i++)
                        {
                            var item = items[i];
                            results.Add(new
                            {
                                Title = item.GetProperty("title").GetString(),
                                Link = item.GetProperty("link").GetString(),
                                Snippet = item.GetProperty("snippet").GetString()
                            });
                        }

                        return new
                        {
                            Query = query,
                            ResultCount = results.Count,
                            Results = results
                        };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error searching the web");
                        return new
                        {
                            Success = false,
                            Error = ex.Message
                        };
                    }
                }
            };
        }
    }
}
