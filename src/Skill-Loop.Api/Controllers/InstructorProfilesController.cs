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

[Route("api/v1/instructor-profiles")]
[Tags("Instructor Profiles")]
public class InstructorProfilesController(ICurrentUser _currentUser) : BaseApiController
{
    /// <summary>
    /// جلب قائمة المدربين المعتمدين (مع دعم الصفحات)
    /// </summary>
    [HttpGet]
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

    /// <summary>
    /// جلب الملف الشخصي لمدرب محدد (عام)
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<IResult> GetProfileByUserId(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetInstructorFullProfileByUserIdQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// جلب الملف الشخصي للمدرب الخاص بالمستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
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

    /// <summary>
    /// إنشاء ملف شخصي كمدرب للمستخدم الحالي
    /// </summary>
    [HttpPost("me")]
    [Authorize]
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

    /// <summary>
    /// تحديث بيانات الملف الشخصي للمدرب (للمستخدم الحالي)
    /// </summary>
    [HttpPut("me")]
    [Authorize]
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

    /// <summary>
    /// تغيير حالة اعتماد المدرب (للمديرين فقط)
    /// </summary>
    [HttpPatch("{userId:guid}/approval-status")]
    [Authorize]
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