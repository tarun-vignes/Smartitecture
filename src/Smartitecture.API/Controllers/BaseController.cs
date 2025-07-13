using Microsoft.AspNetCore.Mvc;
using Smartitecture.Core.Services;
using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger Logger;

        protected BaseController(ILogger logger)
        {
            Logger = logger;
        }

        protected async Task<IActionResult> HandleAsync<TService, TRequest, TResponse>(
            TService service,
            Func<TService, TRequest, CancellationToken, Task<TResponse>> action,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TService : IService<TRequest>
        {
            try
            {
                var result = await action(service, request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in {Controller} handling request", GetType().Name);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
