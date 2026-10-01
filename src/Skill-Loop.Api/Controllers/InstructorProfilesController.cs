using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Api.Contracts.Instructors;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;
using Skill_Loop.Application.Features.Instructors.Commands.CreateMyInstructorProfile;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateMyInstructorProfile;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة الملفات الشخصية للمحاضرين وبياناتهم
/// </summary>
[Route("api/v1/instructor-profiles")]
[Tags("Instructor Profiles")]
[Authorize]
public class InstructorProfilesController(ICurrentUser _currentUser) : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetInstructorsPaged(
        [FromQuery] GetInstructorsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetInstructorsPagedQuery(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.MinRating,
            request.HasCompletedSessions,
            request.SortBy);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetProfileByUserId(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetInstructorFullProfileByUserIdQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("me")]
    public async Task<IResult> GetMyProfile(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Results.Unauthorized();
        }
        var query = new GetInstructorFullProfileByUserIdQuery(_currentUser.UserId.Value);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("me")]
    public async Task<IResult> CreateMyProfile(
        [FromBody] CreateInstructorProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Results.Unauthorized();
        }
        var command = new CreateMyInstructorProfileCommand(
            _currentUser.UserId.Value,
            request.Headline,
            request.Bio);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("me")]
    public async Task<IResult> UpdateMyProfile(
        [FromBody] UpdateInstructorProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Results.Unauthorized();
        }
        var command = new UpdateMyInstructorProfileCommand(
            _currentUser.UserId.Value,
            request.Headline,
            request.Bio);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPatch("{userId:guid}/approval-status")]
    public async Task<IResult> ChangeApprovalStatus(
        [FromRoute] Guid userId,
        [FromBody] ChangeInstructorApprovalStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeInstructorApprovalStatusCommand(userId, request.IsApproved);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}