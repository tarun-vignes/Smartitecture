using System;
using System.Threading.Tasks;
using AIPal.API.Models;
using AIPal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIPal.API.Controllers
{
    /// <summary>
    /// API controller for handling structured queries to the AIPal assistant.
    /// Provides endpoints for processing queries, parsing text to queries, and getting suggestions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IQueryService _queryService;

        /// <summary>
        /// Initializes a new instance of the QueryController.
        /// </summary>
        /// <param name="queryService">The query service for processing structured queries</param>
        public QueryController(IQueryService queryService)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        }

        /// <summary>
        /// Processes a structured query and returns a response.
        /// </summary>
        /// <param name="query">The structured query to process</param>
        /// <returns>A response containing the result of processing the query</returns>
        [HttpPost]
        public async Task<ActionResult<QueryResponseDto>> ProcessQuery([FromBody] QueryDto query)
        {
            if (query == null)
            {
                return BadRequest(new { error = "Query cannot be null" });
            }

            if (string.IsNullOrWhiteSpace(query.Text))
            {
                return BadRequest(new { error = "Query text cannot be empty" });
            }

            var response = await _queryService.ProcessQueryAsync(query);
            return Ok(response);
        }

        /// <summary>
        /// Parses natural language text into a structured query.
        /// </summary>
        /// <param name="request">The request containing the text to parse</param>
        /// <returns>A structured query parsed from the text</returns>
        [HttpPost("parse")]
        public async Task<ActionResult<QueryDto>> ParseText([FromBody] ParseTextRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(new { error = "Text cannot be empty" });
            }

            var query = await _queryService.ParseTextToQueryAsync(request.Text, request.ContextId);
            return Ok(query);
        }

        /// <summary>
        /// Gets suggested queries based on the current context.
        /// </summary>
        /// <param name="contextId">The context ID to get suggestions for</param>
        /// <param name="count">The number of suggestions to return (default: 3)</param>
        /// <returns>A list of suggested queries</returns>
        [HttpGet("suggestions")]
        public async Task<ActionResult<string[]>> GetSuggestions(string contextId, int count = 3)
        {
            if (string.IsNullOrWhiteSpace(contextId))
            {
                return BadRequest(new { error = "Context ID cannot be empty" });
            }

            var suggestions = await _queryService.GetQuerySuggestionsAsync(contextId, count);
            return Ok(suggestions);
        }
    }

    /// <summary>
    /// Request model for parsing text to a structured query.
    /// </summary>
    public class ParseTextRequest
    {
        /// <summary>
        /// Gets or sets the text to parse into a query.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the context ID for maintaining conversation context.
        /// </summary>
        public string ContextId { get; set; }
    }
}
