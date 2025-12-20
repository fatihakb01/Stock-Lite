using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Base controller that provides common functionality for all API controllers.
    /// Includes lazy-loaded access to the Mediator and standardized result handling.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController(IMediator mediator) : ControllerBase
    {
        public IMediator Mediator { get; } = mediator;

        /// <summary>
        /// Standardized handler for formatting and returning results from the application layer.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The result returned from an application request.</param>
        /// <returns>An <see cref="ActionResult"/> with appropriate HTTP status and value.</returns>
        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (!result.IsSuccess) return Problem(title: result.Error, statusCode: result.Code);

            if (result.IsSuccess && result.Value != null) return Ok(result.Value);

            return BadRequest(result.Error);
        }
    }
}
