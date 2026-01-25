using Application.Entities.Core.ScheduledJobs.DTOs;
using Application.Entities.Core.ScheduledJobs.Features.Commands;
using Application.Entities.Core.ScheduledJobs.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ScheduledJobController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduledJobDto>> GetScheduledJob(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetScheduledJob.Query { Id = id }));
    }

    [HttpGet("history/{id}")]
    public async Task<ActionResult<IReadOnlyList<JobExecutionDto>>> GetScheduledJobHistory(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetScheduledJobHistory.Query { Id = id }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost("{id}")]
    public async Task<ActionResult<Unit>> RunScheduledJob(Guid id)
    {
        return HandleResult(await Mediator.Send(new RunScheduledJob.Command { Id = id }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost("order")]
    public async Task<ActionResult<Unit>> ReorderScheduledJobs([FromBody] ReorderScheduledJobsDto dto)
    {
        return HandleResult(await Mediator.Send(new ReorderScheduledJobs.Command { Dto = dto }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ScheduledJobDto>> CreateScheduledJob(
        [FromBody] CreateScheduledJobDto dto)
    {
        return HandleResult(
            await Mediator.Send(
                new CreateScheduledJob.Command { CreateScheduledJobDto = dto }
            )
        );
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ScheduledJobDto>> EditScheduledJob(
        Guid id,
        [FromBody] EditScheduledJobDto dto
    )
    {
        return HandleResult(
            await Mediator.Send(
                new EditScheduledJob.Command { Id = id, EditScheduledJobDto = dto }
            )
        );
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteScheduledJob(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteScheduledJob.Command { Id = id }));
    }
}
