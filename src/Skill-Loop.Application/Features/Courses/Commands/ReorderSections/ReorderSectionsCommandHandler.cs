using Skill_Loop.Application.Common.Errors.Course;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Commands.ReorderSections;

public sealed class ReorderSectionsCommandHandler : ICommandHandler<ReorderSectionsCommand>
{
    private readonly IApplicationDbContext _context;

    public ReorderSectionsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ReorderSectionsCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses
                .Include(c => c.Sections)
                .Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        var result = course.ReorderSections(request.SectionOrders);
        if (result.IsFailure) return result;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
