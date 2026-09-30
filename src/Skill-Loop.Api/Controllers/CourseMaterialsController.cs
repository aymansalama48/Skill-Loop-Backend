using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Courses.Commands.RemoveCourseMaterial;
using Skill_Loop.Application.Features.Courses.Commands.RemoveLessonMaterial;
using Skill_Loop.Application.Features.Courses.Commands.UploadCourseMaterial;
using Skill_Loop.Application.Features.Courses.Commands.UploadLessonMaterial;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/courses/{courseId:guid}")]
[Tags("Course Materials")]
public class CourseMaterialsController : BaseApiController
{
    /// <summary>
    /// إضافة ملف للكورس
    /// </summary>
    [HttpPost("materials")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadCourseMaterial(
        Guid courseId,
        [FromForm] UploadCourseMaterialRequest request,
        CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var command = new UploadCourseMaterialCommand(
            courseId,
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.MaterialType);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف ملف من الكورس
    /// </summary>
    [HttpDelete("materials/{materialId:guid}")]
    [Authorize]
    public async Task<IResult> RemoveCourseMaterial(
        Guid courseId,
        Guid materialId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveCourseMaterialCommand(courseId, materialId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إضافة ملف للدرس
    /// </summary>
    [HttpPost("sections/{sectionId:guid}/lessons/{lessonId:guid}/materials")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadLessonMaterial(
        Guid courseId,
        Guid sectionId,
        Guid lessonId,
        [FromForm] UploadLessonMaterialRequest request,
        CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var command = new UploadLessonMaterialCommand(
            courseId,
            sectionId,
            lessonId,
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.MaterialType);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف ملف من الدرس
    /// </summary>
    [HttpDelete("sections/{sectionId:guid}/lessons/{lessonId:guid}/materials/{materialId:guid}")]
    [Authorize]
    public async Task<IResult> RemoveLessonMaterial(
        Guid courseId,
        Guid sectionId,
        Guid lessonId,
        Guid materialId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveLessonMaterialCommand(courseId, sectionId, lessonId, materialId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
