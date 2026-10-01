namespace Skill_Loop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Instructors;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;
using Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateInstructorReview;
/// <summary>
/// إدارة التقييمات الخاصة بالمحاضرين
/// </summary>
[Route("api/v1/instructor-profiles/{profileId:guid}/reviews")]
[Tags("Instructor Reviews")]
[Authorize]
public class InstructorReviewsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public InstructorReviewsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    [HttpPost]
    public async Task<IResult> AddReview(
        [FromRoute] Guid profileId,
        [FromBody] AddInstructorReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return Results.Unauthorized();
        var command = new AddInstructorReviewCommand(
            profileId,
            _currentUser.UserId.Value,
            request.Rating,
            request.Comment
        );
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("{reviewId:guid}")]
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
    [HttpDelete("{reviewId:guid}")]
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
