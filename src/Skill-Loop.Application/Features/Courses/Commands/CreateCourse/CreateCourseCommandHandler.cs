using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Category;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Domain.Entities.Courses;


namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateCourseCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.AnyAsync(
            _context.Categories.Where(c => c.Id == request.CategoryId),
            cancellationToken);

        if (!categoryExists)
        {
            return Result<Guid>.Failure(CategoryErrors.NotFound);
        }

        // مفيش كورس لسه عشان نعمل عليه ownership check، فالمنع لازم يكون هنا:
        // غير الـ ManageAll لازم behaves كأنه هو المدرب — نتجاهل القيم اللي جاية من الـ client.
        var canManageAll = _currentUser.HasPermission(Permissions.Courses.ManageAll);

        var instructorId = canManageAll ? request.InstructorId : _currentUser.UserId!.Value;
        var instructorName = canManageAll
            ? request.InstructorName
            : _currentUser.FullName ?? string.Empty;

        var courseResult = Course.Create(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Credits,
            request.Level,
            instructorId,
            instructorName,
            request.CategoryId);

        if (courseResult.IsFailure)
        {
            return Result<Guid>.Failure(courseResult.Errors.First());
        }

        var course = courseResult.Data!;
        _context.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}