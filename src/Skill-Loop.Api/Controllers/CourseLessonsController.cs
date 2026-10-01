using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Courses.Commands.AddLesson;
using Skill_Loop.Application.Features.Courses.Commands.RemoveLesson;
using Skill_Loop.Application.Features.Courses.Commands.ReorderLessons;
using Skill_Loop.Application.Features.Courses.Commands.UpdateLesson;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة دروس الكورسات ومحتواها
/// </summary>
[Route("api/v1/courses/{courseId:guid}/sections/{sectionId:guid}/lessons")]
[Tags("Course Lessons")]
[Authorize]
public class CourseLessonsController : BaseApiController
{
    [HttpPost]
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
            request.OrderIndex,
            request.IsPreviewable);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("{lessonId:guid}")]
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
            request.OrderIndex, 
            request.IsPreviewable);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpDelete("{lessonId:guid}")]
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
    [HttpPut("reorder")]
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
    [HttpPost("{lessonId:guid}/video")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadLessonVideo(
        [FromRoute] Guid courseId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid lessonId,
        [FromForm] Skill_Loop.Api.Contracts.Common.UploadFileRequest request,
        CancellationToken cancellationToken)
    {
        var videoFile = request.File;
        if (videoFile == null || videoFile.Length == 0)
        {
            return Results.BadRequest("Video file is required.");
        }

        await using var stream = videoFile.OpenReadStream();
        var command = new Skill_Loop.Application.Features.Courses.Commands.UploadLessonVideo.UploadLessonVideoCommand(
            courseId,
            sectionId,
            lessonId,
            stream,
            videoFile.FileName);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
