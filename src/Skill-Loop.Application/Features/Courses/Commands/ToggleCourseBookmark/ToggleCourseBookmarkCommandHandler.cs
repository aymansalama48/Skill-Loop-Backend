using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Courses.Commands.ToggleCourseBookmark;

public sealed class ToggleCourseBookmarkCommandHandler : ICommandHandler<ToggleCourseBookmarkCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleCourseBookmarkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ToggleCourseBookmarkCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.FirstOrDefaultAsync(
            _context.CourseBookmarks.Where(b => b.UserId == request.UserId && b.CourseId == request.CourseId),
            cancellationToken);

        if (existing != null)
        {
            _context.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(false, "Course removed from bookmarks.");
        }

        var bookmark = CourseBookmark.Create(request.UserId, request.CourseId);
        _context.Add(bookmark);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Course added to bookmarks.");
    }
}