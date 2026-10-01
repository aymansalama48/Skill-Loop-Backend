using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Courses.Commands.RemoveCourseMaterial;
using Skill_Loop.Application.Features.Courses.Commands.RemoveLessonMaterial;
using Skill_Loop.Application.Features.Courses.Commands.UploadCourseMaterial;
using Skill_Loop.Application.Features.Courses.Commands.UploadLessonMaterial;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة المواد التعليمية والملحقات الخاصة بالكورسات
/// </summary>
[Route("api/v1/courses/{courseId:guid}")]
[Tags("Course Materials")]
[Authorize]
public class CourseMaterialsController : BaseApiController
{
    [HttpPost("materials")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadCourseMaterial(
        [FromRoute] Guid courseId,
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
    [HttpDelete("materials/{materialId:guid}")]
    public async Task<IResult> RemoveCourseMaterial(
        [FromRoute] Guid courseId,
        [FromRoute] Guid materialId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveCourseMaterialCommand(courseId, materialId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("sections/{sectionId:guid}/lessons/{lessonId:guid}/materials")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadLessonMaterial(
        [FromRoute] Guid courseId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid lessonId,
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
    [HttpDelete("sections/{sectionId:guid}/lessons/{lessonId:guid}/materials/{materialId:guid}")]
    public async Task<IResult> RemoveLessonMaterial(
        [FromRoute] Guid courseId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid lessonId,
        [FromRoute] Guid materialId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveLessonMaterialCommand(courseId, sectionId, lessonId, materialId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
