using Application.Entities.Core.JobTasks.DTOs;
using Application.Entities.Core.JobTasks.Features.Commands;
using Application.Entities.Core.JobTasks.Features.Queries;
using Application.Entities.Core.ScheduledJobs.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class JobTaskController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("history/{id}")]
    public async Task<ActionResult<IReadOnlyList<JobExecutionDto>>> GetJobTaskHistory(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetJobTaskHistory.Query { Id = id }));
    }

    [HttpGet]
    public async Task<ActionResult<List<JobTaskDto>>> GetJobTasks()
    {
        return HandleResult(await Mediator.Send(new GetJobTasks.Query()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobTaskDetailDto>> GetJobTask(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetJobTask.Query{ Id = id }));
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<IReadOnlyList<JobExecutionDto>>>> GetRecentHistory()
    {
        return HandleResult(await Mediator.Send(new GetRecentHistory.Query()));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("run/{id}")]
    public async Task<ActionResult<Unit>> RunJobTask(Guid id)
    {
        return HandleResult(await Mediator.Send(new RunJobTask.Command { Id = id }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("clean/{id}")]
    public async Task<ActionResult<Unit>> CleanJobTask(Guid id)
    {
        return HandleResult(await Mediator.Send(new CleanJobTask.Command { Id = id }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost]
    public async Task<ActionResult<JobTaskDetailDto>> CreateJobTask([FromBody] CreateJobTaskDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateJobTask.Command { CreateJobTaskDto = dto }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("{id}")]
    public async Task<ActionResult<JobTaskDetailDto>> EditJobTask(
        Guid id,
        [FromBody] EditJobTaskDto dto
    )
    {
        return HandleResult(
            await Mediator.Send(
                new EditJobTask.Command { Id = id, EditJobTaskDto = dto }
            )
        );
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteJobTask(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteJobTask.Command { Id = id }));
    }
}
