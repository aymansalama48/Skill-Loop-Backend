using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Courses.Commands.AddLesson;
using Skill_Loop.Application.Features.Courses.Commands.RemoveLesson;
using Skill_Loop.Application.Features.Courses.Commands.ReorderLessons;
using Skill_Loop.Application.Features.Courses.Commands.UpdateLesson;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/courses/{courseId:guid}/sections/{sectionId:guid}/lessons")]
[Tags("Course Lessons")]
public class CourseLessonsController : BaseApiController
{
    /// <summary>
    /// إضافة درس جديد لقسم داخل الكورس
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IResult> AddLesson(
        Guid courseId,
        Guid sectionId,
        [FromBody] AddLessonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddLessonCommand(
            courseId,
            sectionId,
            request.Title,
            request.VideoUrl,
            request.Duration,
            request.OrderIndex,
            request.IsPreviewable,
            request.StreamingResolution,
            request.ExternalProviderId);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تعديل درس (Lesson)
    /// </summary>
    [HttpPut("{lessonId:guid}")]
    [Authorize]
    public async Task<IResult> UpdateLesson(
        Guid courseId, 
        Guid sectionId, 
        Guid lessonId, 
        [FromBody] UpdateLessonRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonCommand(
            courseId, 
            sectionId, 
            lessonId, 
            request.Title, 
            request.VideoUrl, 
            request.Duration, 
            request.StreamingResolution, 
            request.ExternalProviderId, 
            request.OrderIndex, 
            request.IsPreviewable);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف درس (Lesson)
    /// </summary>
    [HttpDelete("{lessonId:guid}")]
    [Authorize]
    public async Task<IResult> RemoveLesson(
        Guid courseId, 
        Guid sectionId, 
        Guid lessonId, 
        CancellationToken cancellationToken)
    {
        var command = new RemoveLessonCommand(courseId, sectionId, lessonId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إعادة ترتيب الدروس داخل القسم
    /// </summary>
    [HttpPut("reorder")]
    [Authorize]
    public async Task<IResult> ReorderLessons(
        Guid courseId, 
        Guid sectionId, 
        [FromBody] ReorderLessonsRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new ReorderLessonsCommand(courseId, sectionId, request.LessonOrders);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
