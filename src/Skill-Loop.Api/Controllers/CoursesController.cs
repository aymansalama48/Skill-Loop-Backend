using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Courses;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Courses.Commands.ArchiveCourse;
using Skill_Loop.Application.Features.Courses.Commands.CreateCourse;
using Skill_Loop.Application.Features.Courses.Commands.DeleteCourse;
using Skill_Loop.Application.Features.Courses.Commands.PublishCourse;
using Skill_Loop.Application.Features.Courses.Commands.ToggleCourseBookmark;
using Skill_Loop.Application.Features.Courses.Commands.UpdateCourseDetails;
using Skill_Loop.Application.Features.Courses.Queries.GetCourseBookmarks;
using Skill_Loop.Application.Features.Courses.Queries.GetCourseById;
using Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;
using Skill_Loop.Domain.Enums;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة الكورسات (إنشاء، تحديث، ونشر)
/// </summary>
[Route("api/v1/[controller]")]
[Tags("Courses")]
public class CoursesController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public CoursesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
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
            Status = CourseStatus.Published // ثابت: بيجيب المنشور بس
        };
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
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
    [HttpGet("bookmarks")]
    [Authorize]
    public async Task<IResult> GetMyBookmarks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCourseBookmarksQuery(pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("{courseId:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetCourseById(Guid courseId, CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery(courseId);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost]
    [Authorize]
    public async Task<IResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var instructorId = RequireUserId();
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
    [HttpPost("{courseId:guid}/publish")]
    [Authorize]
    public async Task<IResult> PublishCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var command = new PublishCourseCommand(courseId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("{courseId:guid}/bookmark")]
    [Authorize]
    public async Task<IResult> ToggleBookmark(Guid courseId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new ToggleCourseBookmarkCommand(userId, courseId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("{courseId:guid}")]
    [Authorize]
    public async Task<IResult> UpdateCourseDetails(
        Guid courseId,
        [FromBody] UpdateCourseDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourseDetailsCommand(
            courseId,
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Credits,
            request.Level,
            request.CategoryId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("{courseId:guid}/archive")]
    [Authorize]
    public async Task<IResult> ArchiveCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var command = new ArchiveCourseCommand(courseId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpDelete("{courseId:guid}")]
    [Authorize]
    public async Task<IResult> DeleteCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var command = new DeleteCourseCommand(courseId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}