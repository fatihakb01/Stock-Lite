using Application.Entities.Core.ExportProfiles.DTOs;
using Application.Entities.Core.ExportProfiles.Features.Commands;
using Application.Entities.Core.ExportProfiles.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class ExportProfileController(IMediator mediator) : BaseApiController(mediator)
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ExportProfileDto>>> GetExportProfiles()
        {
            return HandleResult(await Mediator.Send(new GetExportProfiles.Query()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IReadOnlyList<ExportProfileDetailDto>>> GetExportProfile(Guid id)
        {
            return HandleResult(await Mediator.Send(new GetExportProfile.Query { Id = id }));
        }

        [Authorize(Policy = "KeyUserOrAdminOrSuperAdmin")]
        [HttpPost("run/{id}")]
        public async Task<ActionResult<Unit>> RunExportProfile(Guid id)
        {
            return HandleResult(await Mediator.Send(new RunExportProfile.Command { Id = id }));
        }

        [Authorize(Policy = "AdminOrSuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<ExportProfileDetailDto>> CreateExportProfile([FromBody] CreateExportProfileDto dto)
        {
            return HandleResult(await Mediator.Send(new CreateExportProfile.Command { Dto = dto }));
        }

        [Authorize(Policy = "AdminOrSuperAdmin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ExportProfileDetailDto>> EditExportProfile(Guid id, [FromBody] EditExportProfileDto dto)
        {
            return HandleResult(await Mediator.Send(new EditExportProfile.Command { Id = id , Dto = dto }));
        }

        [Authorize(Policy = "AdminOrSuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Unit>> DeleteExportProfile(Guid id)
        {
            return HandleResult(await Mediator.Send(new DeleteExportProfile.Command { Id = id }));
        }
    }
}
