using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Courses.Commands.AddCourseReview;
using Skill_Loop.Application.Features.Courses.Queries.GetCourseReviews;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة التقييمات والمراجعات الخاصة بالكورسات
/// </summary>
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
    /// إضافة تقييم جديد للكورس
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IResult> AddReview(
        Guid courseId,
        [FromBody] AddCourseReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new AddCourseReviewCommand(courseId, userId, request.Stars, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    /// <summary>
    /// عرض تقييمات الكورس مع التصفح
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

    /// <summary>
    /// تعديل تقييم مسجل مسبقاً
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<IResult> UpdateReview(
        Guid courseId,
        [FromBody] UpdateCourseReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new Skill_Loop.Application.Features.Courses.Commands.UpdateCourseReview.UpdateCourseReviewCommand(courseId, userId, request.Stars, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف تقييم الكورس
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IResult> DeleteReview(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new Skill_Loop.Application.Features.Courses.Commands.DeleteCourseReview.DeleteCourseReviewCommand(courseId, userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
