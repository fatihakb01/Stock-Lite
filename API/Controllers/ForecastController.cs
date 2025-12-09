using Application.Entities.Derived.Forecasts.Features.Commands;
using Application.Entities.Derived.Forecasts.Features.Queries;
using Domain.Entities.Derived;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ForecastController(IMediator mediator) : BaseApiController(mediator)
    {
        [HttpGet("{periodType}/{productId}")]
        public async Task<ActionResult<Forecast>> GetForecasts(string periodType, Guid productId)
        {
            return HandleResult(await Mediator.Send(new GetForecasts.Query
            {
                ProductId = productId,
                PeriodType = periodType
            }));
        }

        [HttpPost("{companyId}/{periodType?}")]
        public async Task<ActionResult<Forecast>> GenerateForecasts
            (Guid companyId, string? periodType, [FromBody] IEnumerable<Guid>? productIds)
        {
            return HandleResult(await Mediator.Send(new GenerateForecasts.Command
            {
                CompanyId = companyId,
                PeriodType = periodType,
                ProductIds = productIds
            }));
        }

        // To DO: create delete endpoint
    }
}
