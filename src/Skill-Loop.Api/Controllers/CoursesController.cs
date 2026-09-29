using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Courses.Commands.AddCourseReview;
using Skill_Loop.Application.Features.Courses.Commands.AddLesson;
using Skill_Loop.Application.Features.Courses.Commands.AddSection; // تمت إضافة الـ Namespace هنا
using Skill_Loop.Application.Features.Courses.Commands.CreateCourse;
using Skill_Loop.Application.Features.Courses.Commands.PublishCourse;
using Skill_Loop.Application.Features.Courses.Commands.ToggleCourseBookmark;
using Skill_Loop.Application.Features.Courses.Queries.GetCourseById;
using Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;
using Skill_Loop.Application.Features.Courses.Commands.DeleteCourse;
using Skill_Loop.Application.Features.Courses.Commands.ArchiveCourse;
using Skill_Loop.Application.Features.Courses.Commands.UpdateCourseDetails;
using Skill_Loop.Application.Features.Courses.Commands.UpdateSection;
using Skill_Loop.Application.Features.Courses.Commands.RemoveSection;
using Skill_Loop.Application.Features.Courses.Commands.ReorderSections;
using Skill_Loop.Application.Features.Courses.Commands.UpdateLesson;
using Skill_Loop.Application.Features.Courses.Commands.RemoveLesson;
using Skill_Loop.Application.Features.Courses.Commands.ReorderLessons;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/[controller]")]
public class CoursesController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public CoursesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// استرجاع الكورسات المنشورة فقط (الكتالوج الخاص بالطلاب)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetCourses([FromQuery] GetCoursesRequest request, CancellationToken cancellationToken)
    {
        var query = new GetCoursesPagedQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            CategoryId = request.CategoryId,
            Level = request.Level,
            MaxCredits = request.MaxCredits,
            MinRating = request.MinRating,
            SortBy = request.SortBy,
            Status = Skill_Loop.Domain.Enums.CourseStatus.Published // ثابت: بيجيب المنشور بس
        };

        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// استرجاع الكورسات المسودة (Drafts) الخاصة بالمدرب الحالي فقط
    /// </summary>
    [HttpGet("drafts")]
    [Authorize]
    public async Task<IResult> GetMyDraftCourses([FromQuery] GetCoursesRequest request, CancellationToken cancellationToken)
    {
        var query = new GetCoursesPagedQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            CategoryId = request.CategoryId,
            Level = request.Level,
            MaxCredits = request.MaxCredits,
            MinRating = request.MinRating,
            SortBy = request.SortBy,
            Status = CourseStatus.Draft, // ثابت: بيجيب المسودات بس
            InstructorId = _currentUser.UserId // أمان: عشان المدرب ميشوفش مسودات غيره
        };

        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل الكورس ومحتوى الدروس (Syllabus)
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetCourseById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إنشاء كورس جديد (للمحاضرين والمسؤولين)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var instructorId = _currentUser.UserId ?? Guid.Empty;
        var instructorName = _currentUser.FullName ?? "Instructor";

        var command = new CreateCourseCommand(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Credits,
            request.Level,
            instructorId,
            instructorName,
            request.CategoryId);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إضافة قسم جديد (Section) داخل الكورس
    /// </summary>
    [HttpPost("{id:guid}/sections")]
    [Authorize]
    public async Task<IResult> AddSection(
        Guid id,
        [FromBody] AddSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddSectionCommand(id, request.Title, request.OrderIndex);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إضافة درس جديد لقسم داخل الكورس
    /// </summary>
    [HttpPost("{id:guid}/sections/{sectionId:guid}/lessons")]
    [Authorize]
    public async Task<IResult> AddLesson(
        Guid id,
        Guid sectionId,
        [FromBody] AddLessonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddLessonCommand(
            id,
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
    /// نشر الكورس ليصبح متاحاً للطلاب في الكتالوج
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    public async Task<IResult> PublishCourse(Guid id, CancellationToken cancellationToken)
    {
        var command = new PublishCourseCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إضافة تقييم ومراجعة للكورس
    /// </summary>
    [HttpPost("{id:guid}/reviews")]
    [Authorize]
    public async Task<IResult> AddReview(
        Guid id,
        [FromBody] AddCourseReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var command = new AddCourseReviewCommand(id, userId, request.Stars, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حفظ الكورس في المفضلة / إزالته من المفضلة
    /// </summary>
    [HttpPost("{id:guid}/bookmark")]
    [Authorize]
    public async Task<IResult> ToggleBookmark(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var command = new ToggleCourseBookmarkCommand(userId, id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تعديل تفاصيل الكورس
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IResult> UpdateCourseDetails(
        Guid id,
        [FromBody] UpdateCourseDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourseDetailsCommand(
            id,
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Credits,
            request.Level,
            request.CategoryId);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// أرشفة الكورس (تغيير حالته إلى Archived)
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [Authorize]
    public async Task<IResult> ArchiveCourse(Guid id, CancellationToken cancellationToken)
    {
        var command = new ArchiveCourseCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف الكورس (Soft Delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IResult> DeleteCourse(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCourseCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تعديل قسم (Section)
    /// </summary>
    [HttpPut("{id:guid}/sections/{sectionId:guid}")]
    [Authorize]
    public async Task<IResult> UpdateSection(
        Guid id, 
        Guid sectionId, 
        [FromBody] UpdateSectionRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new UpdateSectionCommand(id, sectionId, request.Title, request.OrderIndex);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف قسم (Section)
    /// </summary>
    [HttpDelete("{id:guid}/sections/{sectionId:guid}")]
    [Authorize]
    public async Task<IResult> RemoveSection(
        Guid id, 
        Guid sectionId, 
        CancellationToken cancellationToken)
    {
        var command = new RemoveSectionCommand(id, sectionId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إعادة ترتيب الأقسام
    /// </summary>
    [HttpPut("{id:guid}/sections/reorder")]
    [Authorize]
    public async Task<IResult> ReorderSections(
        Guid id, 
        [FromBody] ReorderSectionsRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new ReorderSectionsCommand(id, request.SectionOrders);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تعديل درس (Lesson)
    /// </summary>
    [HttpPut("{id:guid}/sections/{sectionId:guid}/lessons/{lessonId:guid}")]
    [Authorize]
    public async Task<IResult> UpdateLesson(
        Guid id, 
        Guid sectionId, 
        Guid lessonId, 
        [FromBody] UpdateLessonRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonCommand(
            id, 
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
    [HttpDelete("{id:guid}/sections/{sectionId:guid}/lessons/{lessonId:guid}")]
    [Authorize]
    public async Task<IResult> RemoveLesson(
        Guid id, 
        Guid sectionId, 
        Guid lessonId, 
        CancellationToken cancellationToken)
    {
        var command = new RemoveLessonCommand(id, sectionId, lessonId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إعادة ترتيب الدروس داخل القسم
    /// </summary>
    [HttpPut("{id:guid}/sections/{sectionId:guid}/lessons/reorder")]
    [Authorize]
    public async Task<IResult> ReorderLessons(
        Guid id, 
        Guid sectionId, 
        [FromBody] ReorderLessonsRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new ReorderLessonsCommand(id, sectionId, request.LessonOrders);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}