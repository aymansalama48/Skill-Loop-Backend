using Skill_Loop.Application.Common.Errors.Course;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateLesson;

public sealed class UpdateLessonCommandHandler : ICommandHandler<UpdateLessonCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateLessonCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        var result = course.UpdateLesson(
            request.SectionId, 
            request.LessonId, 
            request.Title, 
            request.OrderIndex, 
            request.IsPreviewable);

        if (result.IsFailure) return result;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
