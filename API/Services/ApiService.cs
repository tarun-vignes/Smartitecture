using System;
using System.Threading.Tasks;
using AIPal.API.Models;
using AIPal.Services;

namespace AIPal.API.Services
{
    /// <summary>
    /// Implementation of the API service that handles chat interactions.
    /// Provides a higher-level abstraction over the LLM service and command execution.
    /// </summary>
    public class ApiService : IApiService
    {
        private readonly ILLMService _llmService;
        private readonly CommandMapper _commandMapper;

        /// <summary>
        /// Initializes a new instance of the ApiService.
        /// </summary>
        /// <param name="llmService">The language model service for processing messages</param>
        /// <param name="commandMapper">The command mapper for executing system commands</param>
        public ApiService(ILLMService llmService, CommandMapper commandMapper)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _commandMapper = commandMapper ?? throw new ArgumentNullException(nameof(commandMapper));
        }

        /// <summary>
        /// Processes a chat request, handling both conversation and command execution.
        /// </summary>
        /// <param name="request">The chat request containing the message and processing options</param>
        /// <returns>A response with the AI's reply and command execution details if applicable</returns>
        public async Task<ChatResponseDto> ProcessChatRequestAsync(ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException("Message cannot be empty", nameof(request));
            }

            var response = new ChatResponseDto();

            // Try to parse and execute a command if requested
            if (request.ParseAsCommand)
            {
                var (commandName, parameters) = await _llmService.ParseCommandAsync(request.Message);
                
                if (commandName != "Unknown")
                {
                    response.CommandExecuted = true;
                    response.CommandName = commandName;
                    response.CommandSuccess = await _commandMapper.ExecuteCommandAsync(commandName, parameters);
                    
                    // If command was executed, provide a response about the execution
                    if (response.CommandSuccess)
                    {
                        response.Response = $"Successfully executed {commandName} command.";
                    }
                    else
                    {
                        response.Response = $"Failed to execute {commandName} command.";
                    }
                    
                    return response;
                }
            }

            // If no command was executed or parsing wasn't requested, get a conversational response
            response.Response = await _llmService.GetResponseAsync(request.Message);
            return response;
        }
    }
}
