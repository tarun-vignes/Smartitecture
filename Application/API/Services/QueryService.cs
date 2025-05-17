using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIPal.API.Models;
using AIPal.Commands;
using AIPal.Services;

namespace AIPal.API.Services
{
    /// <summary>
    /// Implementation of the query service that handles structured queries to the AI assistant.
    /// Provides advanced query processing, context management, and integration with system commands.
    /// </summary>
    public class QueryService : IQueryService
    {
        private readonly ILLMService _llmService;
        private readonly CommandMapper _commandMapper;
        private readonly Dictionary<string, List<QueryDto>> _queryContexts = new Dictionary<string, List<QueryDto>>();

        /// <summary>
        /// Initializes a new instance of the QueryService.
        /// </summary>
        /// <param name="llmService">The language model service for processing queries</param>
        /// <param name="commandMapper">The command mapper for executing system commands</param>
        public QueryService(ILLMService llmService, CommandMapper commandMapper)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _commandMapper = commandMapper ?? throw new ArgumentNullException(nameof(commandMapper));
        }

        /// <summary>
        /// Processes a structured query and returns a response.
        /// Handles different query types and maintains conversation context.
        /// </summary>
        /// <param name="query">The structured query to process</param>
        /// <returns>A response containing the result of processing the query</returns>
        public async Task<QueryResponseDto> ProcessQueryAsync(QueryDto query)
        {
            // Create a response object
            var response = new QueryResponseDto
            {
                QueryId = query.Id,
                ContextId = query.ContextId,
                Status = "success"
            };

            try
            {
                // Store the query in the context if a context ID is provided
                if (!string.IsNullOrEmpty(query.ContextId))
                {
                    if (!_queryContexts.ContainsKey(query.ContextId))
                    {
                        _queryContexts[query.ContextId] = new List<QueryDto>();
                    }
                    
                    _queryContexts[query.ContextId].Add(query);
                }

                // Process based on query type
                switch (query.Type?.ToLower())
                {
                    case "command":
                        await ProcessCommandQuery(query, response);
                        break;

                    case "question":
                        await ProcessQuestionQuery(query, response);
                        break;

                    case "search":
                        await ProcessSearchQuery(query, response);
                        break;

                    default:
                        // Default to treating it as a question
                        await ProcessQuestionQuery(query, response);
                        break;
                }

                // Generate suggestions for follow-up queries
                response.Suggestions = await GenerateQuerySuggestions(query, response);
            }
            catch (Exception ex)
            {
                response.Status = "failed";
                response.Response = $"Error processing query: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Converts natural language text input into a structured query.
        /// Uses the LLM service to understand the intent and extract parameters.
        /// </summary>
        /// <param name="text">The natural language input</param>
        /// <param name="contextId">Optional context ID for maintaining conversation context</param>
        /// <returns>A structured query parsed from the natural language input</returns>
        public async Task<QueryDto> ParseTextToQueryAsync(string text, string contextId = null)
        {
            // Create a basic query with the text
            var query = new QueryDto
            {
                Text = text,
                ContextId = contextId,
                Type = "unknown"
            };

            try
            {
                // First, try to parse as a command
                var (commandName, parameters) = await _llmService.ParseCommandAsync(text);
                
                if (commandName != "Unknown")
                {
                    // This is a command query
                    query.Type = "command";
                    query.Action = commandName;
                    
                    // Convert parameters to dictionary
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        query.Parameters[$"param{i + 1}"] = parameters[i];
                    }
                    
                    return query;
                }

                // If not a command, use a more sophisticated prompt to determine query type and extract parameters
                // This would typically use a more complex LLM prompt to analyze the text
                // For now, we'll use a simplified approach
                
                if (text.Contains("?") || 
                    text.StartsWith("what", StringComparison.OrdinalIgnoreCase) ||
                    text.StartsWith("who", StringComparison.OrdinalIgnoreCase) ||
                    text.StartsWith("when", StringComparison.OrdinalIgnoreCase) ||
                    text.StartsWith("where", StringComparison.OrdinalIgnoreCase) ||
                    text.StartsWith("why", StringComparison.OrdinalIgnoreCase) ||
                    text.StartsWith("how", StringComparison.OrdinalIgnoreCase))
                {
                    query.Type = "question";
                    query.Action = "answer";
                }
                else if (text.StartsWith("search", StringComparison.OrdinalIgnoreCase) ||
                         text.StartsWith("find", StringComparison.OrdinalIgnoreCase) ||
                         text.StartsWith("look for", StringComparison.OrdinalIgnoreCase))
                {
                    query.Type = "search";
                    query.Action = "search";
                    
                    // Extract search terms (simplified)
                    var searchTerms = text.Split(' ').Skip(1).ToArray();
                    query.Parameters["terms"] = string.Join(" ", searchTerms);
                }
                else
                {
                    // Default to question if we can't determine the type
                    query.Type = "question";
                    query.Action = "answer";
                }
            }
            catch (Exception)
            {
                // If parsing fails, default to treating it as a question
                query.Type = "question";
                query.Action = "answer";
            }

            return query;
        }

        /// <summary>
        /// Gets suggested queries based on the current context.
        /// </summary>
        /// <param name="contextId">The context ID to get suggestions for</param>
        /// <param name="count">The number of suggestions to return</param>
        /// <returns>A list of suggested queries</returns>
        public async Task<string[]> GetQuerySuggestionsAsync(string contextId, int count = 3)
        {
            // If no context or the context doesn't exist, return generic suggestions
            if (string.IsNullOrEmpty(contextId) || !_queryContexts.ContainsKey(contextId))
            {
                return new[]
                {
                    "What can you help me with?",
                    "Open File Explorer",
                    "What's the weather like today?"
                };
            }

            // Get the context queries
            var contextQueries = _queryContexts[contextId];
            
            // If there are no queries in the context, return generic suggestions
            if (contextQueries.Count == 0)
            {
                return new[]
                {
                    "What can you help me with?",
                    "Open File Explorer",
                    "What's the weather like today?"
                };
            }

            // Get the most recent query
            var lastQuery = contextQueries.Last();
            
            // Generate suggestions based on the last query
            // This would typically use a more complex LLM prompt
            // For now, we'll use a simplified approach based on query type
            
            switch (lastQuery.Type?.ToLower())
            {
                case "command":
                    return new[]
                    {
                        "Did that work correctly?",
                        $"Show me more about {lastQuery.Action}",
                        "What other commands can you run?"
                    };

                case "question":
                    return new[]
                    {
                        "Tell me more about that",
                        "Why is that the case?",
                        "Can you explain it differently?"
                    };

                case "search":
                    return new[]
                    {
                        "Show me more results",
                        "Refine the search",
                        "Save these results"
                    };

                default:
                    return new[]
                    {
                        "What else can you help me with?",
                        "Tell me more",
                        "What are your capabilities?"
                    };
            }
        }

        /// <summary>
        /// Processes a command query by executing the specified command.
        /// </summary>
        /// <param name="query">The command query to process</param>
        /// <param name="response">The response to update with the command results</param>
        private async Task ProcessCommandQuery(QueryDto query, QueryResponseDto response)
        {
            // Extract command parameters
            var parameters = new List<string>();
            foreach (var key in query.Parameters.Keys.OrderBy(k => k))
            {
                parameters.Add(query.Parameters[key].ToString());
            }

            // Execute the command
            var success = await _commandMapper.ExecuteCommandAsync(query.Action, parameters.ToArray());
            
            // Create an action record
            var action = new QueryAction
            {
                Type = "command",
                Name = query.Action,
                Success = success,
                Result = success ? "Command executed successfully" : "Command execution failed"
            };
            
            response.Actions.Add(action);
            
            // Set the response text
            if (success)
            {
                response.Response = $"I've executed the {query.Action} command for you.";
            }
            else
            {
                response.Response = $"I couldn't execute the {query.Action} command. Please check the parameters and try again.";
                response.Status = "failed";
            }
        }

        /// <summary>
        /// Processes a question query by getting a response from the LLM.
        /// </summary>
        /// <param name="query">The question query to process</param>
        /// <param name="response">The response to update with the answer</param>
        private async Task ProcessQuestionQuery(QueryDto query, QueryResponseDto response)
        {
            // Get context information if available
            string contextPrompt = "";
            if (!string.IsNullOrEmpty(query.ContextId) && _queryContexts.ContainsKey(query.ContextId))
            {
                var contextQueries = _queryContexts[query.ContextId];
                if (contextQueries.Count > 0)
                {
                    // Add the last few queries as context
                    var recentQueries = contextQueries.Skip(Math.Max(0, contextQueries.Count - 3)).ToList();
                    contextPrompt = "Previous conversation:\n";
                    foreach (var prevQuery in recentQueries)
                    {
                        contextPrompt += $"User: {prevQuery.Text}\n";
                    }
                    contextPrompt += "\n";
                }
            }

            // Combine context and query
            var fullPrompt = $"{contextPrompt}User: {query.Text}";
            
            // Get response from LLM
            var llmResponse = await _llmService.GetResponseAsync(fullPrompt);
            
            // Set the response
            response.Response = llmResponse;
            
            // Add an action record
            response.Actions.Add(new QueryAction
            {
                Type = "question",
                Name = "answer",
                Success = true,
                Result = "Answer provided"
            });
        }

        /// <summary>
        /// Processes a search query by performing a search operation.
        /// </summary>
        /// <param name="query">The search query to process</param>
        /// <param name="response">The response to update with the search results</param>
        private async Task ProcessSearchQuery(QueryDto query, QueryResponseDto response)
        {
            // Extract search terms
            string searchTerms = query.Parameters.ContainsKey("terms") 
                ? query.Parameters["terms"].ToString() 
                : query.Text;

            // For now, we'll just use the LLM to generate a response about the search
            // In a real implementation, this would connect to a search provider
            var searchPrompt = $"The user wants to search for: {searchTerms}. Provide a response as if you've searched for this information.";
            var llmResponse = await _llmService.GetResponseAsync(searchPrompt);
            
            // Set the response
            response.Response = llmResponse;
            
            // Add an action record
            response.Actions.Add(new QueryAction
            {
                Type = "search",
                Name = "search",
                Success = true,
                Result = "Search results provided"
            });
            
            // In a real implementation, we would also set structured data
            // response.Data = searchResults;
        }

        /// <summary>
        /// Generates suggestions for follow-up queries based on the current query and response.
        /// </summary>
        /// <param name="query">The current query</param>
        /// <param name="response">The response to the query</param>
        /// <returns>A list of suggested follow-up queries</returns>
        private async Task<List<string>> GenerateQuerySuggestions(QueryDto query, QueryResponseDto response)
        {
            // For now, we'll return predefined suggestions based on query type
            // In a real implementation, this would use the LLM to generate contextual suggestions
            
            switch (query.Type?.ToLower())
            {
                case "command":
                    return new List<string>
                    {
                        "Did that work correctly?",
                        $"Tell me more about {query.Action}",
                        "What other commands can you run?"
                    };

                case "question":
                    return new List<string>
                    {
                        "Can you explain that in more detail?",
                        "Why is that important?",
                        "How does that relate to Windows?"
                    };

                case "search":
                    return new List<string>
                    {
                        "Can you show more specific results?",
                        "How can I learn more about this topic?",
                        "Save this information for later"
                    };

                default:
                    return new List<string>
                    {
                        "What else can you help me with?",
                        "Tell me about your capabilities",
                        "How can I customize Windows settings?"
                    };
            }
        }
    }
}
