using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Smartitecture.Services
{
    /// <summary>
    /// Implementation of the IAgentService using Azure OpenAI's function calling capabilities.
    /// </summary>
    public class AzureOpenAIAgentService : IAgentService
    {
        private readonly OpenAIClient _client;
        private readonly ILogger<AzureOpenAIAgentService> _logger;
        private readonly AzureOpenAIOptions _options;
        private readonly List<AgentTool> _tools = new List<AgentTool>();
        private readonly Dictionary<string, List<ChatMessage>> _conversationHistory = new Dictionary<string, List<ChatMessage>>();

        /// <summary>
        /// Initializes a new instance of the AzureOpenAIAgentService class.
        /// </summary>
        /// <param name="client">The Azure OpenAI client</param>
        /// <param name="options">The Azure OpenAI configuration options</param>
        /// <param name="logger">The logger</param>
        public AzureOpenAIAgentService(
            OpenAIClient client,
            IOptions<AzureOpenAIOptions> options,
            ILogger<AzureOpenAIAgentService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void RegisterTool(AgentTool tool)
        {
            if (tool == null)
            {
                throw new ArgumentNullException(nameof(tool));
            }

            // Remove any existing tool with the same name to avoid duplicates
            _tools.RemoveAll(t => t.Name == tool.Name);
            _tools.Add(tool);
            _logger.LogInformation("Registered tool: {ToolName}", tool.Name);
        }

        /// <inheritdoc/>
        public IReadOnlyList<AgentTool> GetAvailableTools()
        {
            return _tools.AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<AgentResponse> ProcessRequestAsync(string userInput, string contextId = null)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                throw new ArgumentException("User input cannot be empty", nameof(userInput));
            }

            var response = new AgentResponse();
            
            try
            {
                // Get or create conversation history
                if (string.IsNullOrEmpty(contextId))
                {
                    contextId = Guid.NewGuid().ToString();
                }

                if (!_conversationHistory.ContainsKey(contextId))
                {
                    _conversationHistory[contextId] = new List<ChatMessage>
                    {
                        new ChatMessage(ChatRole.System, "You are an AI assistant that can use tools to help users accomplish tasks. " +
                                                        "Think step-by-step about what the user is asking and use the appropriate tools when necessary.")
                    };
                }

                // Add user message to history
                _conversationHistory[contextId].Add(new ChatMessage(ChatRole.User, userInput));

                // Convert our tools to Azure OpenAI function definitions
                var functionDefinitions = _tools.Select(tool => new FunctionDefinition
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    Parameters = BinaryData.FromObjectAsJson(new
                    {
                        type = "object",
                        properties = tool.Parameters.ToDictionary(
                            p => p.Key,
                            p => new
                            {
                                type = p.Value.Type,
                                description = p.Value.Description
                            }),
                        required = tool.Parameters.Where(p => p.Value.Required).Select(p => p.Key).ToArray()
                    })
                }).ToList();

                // Process the conversation with the model
                bool continueProcessing = true;
                int maxIterations = 5; // Prevent infinite loops
                int iterations = 0;

                while (continueProcessing && iterations < maxIterations)
                {
                    iterations++;
                    
                    // Create the chat completion options
                    var chatCompletionOptions = new ChatCompletionsOptions
                    {
                        DeploymentName = _options.DeploymentName,
                        Temperature = 0.2f, // Lower temperature for more deterministic responses
                        MaxTokens = 1000
                    };

                    // Add messages from conversation history
                    foreach (var message in _conversationHistory[contextId])
                    {
                        chatCompletionOptions.Messages.Add(message);
                    }

                    // Add function definitions
                    foreach (var function in functionDefinitions)
                    {
                        chatCompletionOptions.Functions.Add(function);
                    }

                    // Get completion from Azure OpenAI
                    var completionResult = await _client.GetChatCompletionsAsync(chatCompletionOptions);
                    var completion = completionResult.Value;
                    var responseMessage = completion.Choices[0].Message;

                    // Add the assistant's message to the conversation history
                    _conversationHistory[contextId].Add(responseMessage);

                    // Check if the model wants to call a function
                    if (responseMessage.FunctionCall != null)
                    {
                        var functionCall = responseMessage.FunctionCall;
                        var toolName = functionCall.Name;
                        var toolParameters = JsonSerializer.Deserialize<Dictionary<string, object>>(functionCall.Arguments);
                        
                        // Find the tool
                        var tool = _tools.FirstOrDefault(t => t.Name == toolName);
                        
                        if (tool != null)
                        {
                            // Create an action record
                            var action = new AgentAction
                            {
                                ToolName = toolName,
                                Input = toolParameters
                            };

                            try
                            {
                                // Execute the tool
                                var toolResult = await tool.Execute(toolParameters);
                                action.Output = toolResult;
                                action.Success = true;

                                // Add the function result to the conversation
                                _conversationHistory[contextId].Add(new ChatMessage(ChatRole.Function, 
                                    JsonSerializer.Serialize(toolResult), 
                                    functionCall.Name));
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error executing tool {ToolName}", toolName);
                                action.Success = false;
                                action.Output = $"Error: {ex.Message}";

                                // Add the error to the conversation
                                _conversationHistory[contextId].Add(new ChatMessage(ChatRole.Function, 
                                    $"{{\"error\": \"{ex.Message}\"}}",
                                    functionCall.Name));
                            }

                            // Add the action to the response
                            response.Actions.Add(action);
                        }
                        else
                        {
                            _logger.LogWarning("Tool {ToolName} not found", toolName);
                            _conversationHistory[contextId].Add(new ChatMessage(ChatRole.Function, 
                                "{\"error\": \"Tool not found\"}",
                                functionCall.Name));
                        }
                    }
                    else
                    {
                        // The model provided a final answer
                        response.Response = responseMessage.Content;
                        continueProcessing = false;
                    }
                }

                // If we reached the max iterations, get a final response
                if (iterations >= maxIterations && string.IsNullOrEmpty(response.Response))
                {
                    // Ask the model for a final summary
                    _conversationHistory[contextId].Add(new ChatMessage(ChatRole.User, 
                        "Please provide a final summary of what you've done and what you've found."));
                    
                    var finalOptions = new ChatCompletionsOptions
                    {
                        DeploymentName = _options.DeploymentName,
                        Temperature = 0.2f,
                        MaxTokens = 1000
                    };

                    foreach (var message in _conversationHistory[contextId])
                    {
                        finalOptions.Messages.Add(message);
                    }

                    var finalResult = await _client.GetChatCompletionsAsync(finalOptions);
                    response.Response = finalResult.Value.Choices[0].Message.Content;
                }

                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing agent request");
                response.Success = false;
                response.ErrorMessage = $"Error processing request: {ex.Message}";
            }

            return response;
        }
    }
}
