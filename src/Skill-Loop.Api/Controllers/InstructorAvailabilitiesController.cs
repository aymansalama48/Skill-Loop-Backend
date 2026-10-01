namespace Skill_Loop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Instructors;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;
using Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorAvailability;
/// <summary>
/// إدارة أوقات الفراغ والمواعيد المتاحة للمحاضرين
/// </summary>
[Route("api/v1/instructor-profiles/{profileId:guid}/availabilities")]
[Tags("Instructor Availabilities")]
[Authorize]
public class InstructorAvailabilitiesController : BaseApiController
{
    [HttpPost]
    public async Task<IResult> AddAvailability(
        [FromRoute] Guid profileId,
        [FromBody] AddAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddInstructorAvailabilityCommand(
            profileId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime
        );
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpDelete("{availabilityId:guid}")]
    public async Task<IResult> RemoveAvailability(
        [FromRoute] Guid profileId,
        [FromRoute] Guid availabilityId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveInstructorAvailabilityCommand(
            profileId,
            availabilityId
        );
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
