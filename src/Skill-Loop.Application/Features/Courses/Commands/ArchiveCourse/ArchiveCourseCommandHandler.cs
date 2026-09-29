using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Commands.ArchiveCourse;

public sealed class ArchiveCourseCommandHandler : ICommandHandler<ArchiveCourseCommand>
{
    private readonly IApplicationDbContext _context;

    public ArchiveCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ArchiveCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(new Error("Course.NotFound", "Course was not found.", ErrorType.NotFound));
        }

        course.Archive();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
