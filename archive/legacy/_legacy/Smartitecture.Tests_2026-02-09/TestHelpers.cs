using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;

namespace Smartitecture.Tests
{
    /// <summary>
    /// Provides helper methods and utilities for testing
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a test configuration with the specified settings
        /// </summary>
        /// <param name="initialData">Optional dictionary of configuration values</param>
        /// <returns>IConfiguration instance for testing</returns>
        public static IConfiguration CreateTestConfiguration(Dictionary<string, string> initialData = null)
        {
            var configBuilder = new ConfigurationBuilder();
            
            // Add appsettings.test.json if it exists
            string testSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.test.json");
            if (File.Exists(testSettingsPath))
            {
                configBuilder.AddJsonFile(testSettingsPath);
            }
            
            // Add in-memory collection if provided
            if (initialData != null)
            {
                configBuilder.AddInMemoryCollection(initialData);
            }
            
            return configBuilder.Build();
        }
        
        /// <summary>
        /// Creates a mock configuration section with the specified key-value pairs
        /// </summary>
        /// <param name="keyValuePairs">Dictionary of configuration values</param>
        /// <returns>IConfiguration instance</returns>
        public static IConfiguration CreateMockConfiguration(Dictionary<string, Dictionary<string, string>> keyValuePairs)
        {
            var configBuilder = new ConfigurationBuilder();
            var memoryDict = new Dictionary<string, string>();
            
            foreach (var section in keyValuePairs)
            {
                foreach (var kvp in section.Value)
                {
                    memoryDict.Add($"{section.Key}:{kvp.Key}", kvp.Value);
                }
            }
            
            configBuilder.AddInMemoryCollection(memoryDict);
            return configBuilder.Build();
        }
    }
}
