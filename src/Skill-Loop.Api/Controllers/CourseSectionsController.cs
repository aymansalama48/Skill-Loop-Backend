using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Courses.Commands.AddSection;
using Skill_Loop.Application.Features.Courses.Commands.RemoveSection;
using Skill_Loop.Application.Features.Courses.Commands.ReorderSections;
using Skill_Loop.Application.Features.Courses.Commands.UpdateSection;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/courses/{courseId:guid}/sections")]
[Tags("Course Sections")]
public class CourseSectionsController : BaseApiController
{
    /// <summary>
    /// إضافة قسم جديد (Section) داخل الكورس
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IResult> AddSection(
        Guid courseId,
        [FromBody] AddSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddSectionCommand(courseId, request.Title, request.OrderIndex);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تعديل قسم (Section)
    /// </summary>
    [HttpPut("{sectionId:guid}")]
    [Authorize]
    public async Task<IResult> UpdateSection(
        Guid courseId, 
        Guid sectionId, 
        [FromBody] UpdateSectionRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new UpdateSectionCommand(courseId, sectionId, request.Title, request.OrderIndex);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف قسم (Section)
    /// </summary>
    [HttpDelete("{sectionId:guid}")]
    [Authorize]
    public async Task<IResult> RemoveSection(
        Guid courseId, 
        Guid sectionId, 
        CancellationToken cancellationToken)
    {
        var command = new RemoveSectionCommand(courseId, sectionId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إعادة ترتيب الأقسام
    /// </summary>
    [HttpPut("reorder")]
    [Authorize]
    public async Task<IResult> ReorderSections(
        Guid courseId, 
        [FromBody] ReorderSectionsRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new ReorderSectionsCommand(courseId, request.SectionOrders);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
