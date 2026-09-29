using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Commands.ReorderLessons;

public sealed class ReorderLessonsCommandHandler : ICommandHandler<ReorderLessonsCommand>
{
    private readonly IApplicationDbContext _context;

    public ReorderLessonsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ReorderLessonsCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(new Error("Course.NotFound", "Course was not found.", ErrorType.NotFound));
        }

        var result = course.ReorderLessons(request.SectionId, request.LessonOrders);
        if (result.IsFailure) return result;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
