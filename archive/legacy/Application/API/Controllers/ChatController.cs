using System;
using System.Threading.Tasks;
using Smartitecture.API.Models;
using Smartitecture.Services;
using Microsoft.AspNetCore.Mvc;

namespace Smartitecture.API.Controllers
{
    /// <summary>
    /// API controller for handling chat interactions with Smartitecture.
    /// Provides endpoints for sending messages and receiving responses.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILLMService _llmService;
        private readonly CommandMapper _commandMapper;

        /// <summary>
        /// Initializes a new instance of the ChatController.
        /// </summary>
        /// <param name="llmService">The language model service for processing messages</param>
        /// <param name="commandMapper">The command mapper for executing system commands</param>
        public ChatController(ILLMService llmService, CommandMapper commandMapper)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _commandMapper = commandMapper ?? throw new ArgumentNullException(nameof(commandMapper));
        }

        /// <summary>
        /// Processes a chat message, potentially executing commands if requested.
        /// </summary>
        /// <param name="request">The chat request containing the message and processing options</param>
        /// <returns>A response with the AI's reply and command execution details if applicable</returns>
        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> ProcessMessage([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
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
                    
                    return Ok(response);
                }
            }

            // If no command was executed or parsing wasn't requested, get a conversational response
            response.Response = await _llmService.GetResponseAsync(request.Message);
            return Ok(response);
        }
    }
}
