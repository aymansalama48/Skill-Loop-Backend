using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Api.Contracts.Instructors;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;
using Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;
using Skill_Loop.Application.Features.Instructors.Commands.CreateMyInstructorProfile;
using Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateInstructorReview;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateMyInstructorProfile;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;

namespace Skill_Loop.Api.Controllers;

[Route("api/instructor-profiles")]
public class InstructorProfilesController(ICurrentUser _currentUser) : BaseApiController
{
    /// <summary>
    /// جلب قائمة المدربين المعتمدين (مع دعم الصفحات)
    /// </summary>
    /// <summary>
    /// جلب قائمة المدربين المعتمدين (مع دعم الفلترة والصفحات)
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
    /// إضافة تقييم لمدرب
    /// </summary>
    [HttpPost("{profileId:guid}/reviews")]
    [Authorize]
    public async Task<IResult> AddReview(
        [FromRoute] Guid profileId,
        [FromBody] AddInstructorReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Results.Unauthorized();
        }

        var command = new AddInstructorReviewCommand(
            profileId,
            _currentUser.UserId.Value,
            request.Rating,
            request.Comment
        );

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
    /// <summary>
    /// تغيير حالة اعتماد المدرب (للمديرين فقط)
    /// </summary>
    [HttpPatch("users/{userId:guid}/approval-status")]
    // TODO: أضف صلاحية الآدمن هنا مثل [Permission(Permissions.Users.Activate)]
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
    /// <summary>
    /// تعديل تقييم مدرب
    /// </summary>
    [HttpPut("{profileId:guid}/reviews/{reviewId:guid}")]
    [Authorize]
    public async Task<IResult> UpdateReview(
        [FromRoute] Guid profileId,
        [FromRoute] Guid reviewId,
        [FromBody] UpdateInstructorReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return Results.Unauthorized();

        var command = new UpdateInstructorReviewCommand(
            profileId,
            reviewId,
            _currentUser.UserId.Value,
            request.Rating,
            request.Comment
        );

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف تقييم مدرب
    /// </summary>
    [HttpDelete("{profileId:guid}/reviews/{reviewId:guid}")]
    [Authorize]
    public async Task<IResult> RemoveReview(
        [FromRoute] Guid profileId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return Results.Unauthorized();

        var command = new RemoveInstructorReviewCommand(
            profileId,
            reviewId,
            _currentUser.UserId.Value
        );

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}