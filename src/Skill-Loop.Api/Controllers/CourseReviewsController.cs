using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Courses.Commands.AddCourseReview;
using Skill_Loop.Application.Features.Courses.Queries.GetCourseReviews;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/courses/{courseId:guid}/reviews")]
[Tags("Course Reviews")]
public class CourseReviewsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public CourseReviewsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// إضافة تقييم ومراجعة للكورس
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IResult> AddReview(
        Guid courseId,
        [FromBody] AddCourseReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var command = new AddCourseReviewCommand(courseId, userId, request.Stars, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// استرجاع تقييمات الكورس
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetReviews(
        Guid courseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCourseReviewsQuery(courseId, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
