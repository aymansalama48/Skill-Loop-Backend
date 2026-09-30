using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Enrollments;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Enrollments.Commands.EnrollInCourse;
using Skill_Loop.Application.Features.Enrollments.Commands.UpdateLessonProgress;
using Skill_Loop.Application.Features.Enrollments.Queries.GetUserEnrolledCourses;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة اشتراكات الطلاب في الكورسات
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class EnrollmentsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public EnrollmentsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    [HttpPost("enroll")]
    public async Task<IResult> EnrollInCourse([FromBody] EnrollInCourseRequest request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new EnrollInCourseCommand(userId, request.CourseId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("my-courses")]
    public async Task<IResult> GetMyCourses(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var query = new GetUserEnrolledCoursesQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("{courseId:guid}/lessons/progress")]
    public async Task<IResult> UpdateProgress(
        Guid courseId,
        [FromBody] UpdateLessonProgressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new UpdateLessonProgressCommand(userId, courseId, request.LessonId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("{courseId:guid}/lessons/{lessonId:guid}/complete")]
    public async Task<IResult> CompleteLesson(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new UpdateLessonProgressCommand(userId, courseId, lessonId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
