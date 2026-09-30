using Skill_Loop.Application.Common.Errors.LessonMaterial;
using Skill_Loop.Application.Common.Errors.Lesson;
using Skill_Loop.Application.Common.Errors.Section;
using Skill_Loop.Application.Common.Errors.Course;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Commands.RemoveLessonMaterial;

public sealed class RemoveLessonMaterialCommandHandler : ICommandHandler<RemoveLessonMaterialCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICourseContentStorage _storage;

    public RemoveLessonMaterialCommandHandler(IApplicationDbContext context, ICourseContentStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Result> Handle(RemoveLessonMaterialCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .ThenInclude(l => l.Resources)
                .Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
            return Result.Failure(CourseErrors.NotFound);

        var section = course.Sections.FirstOrDefault(s => s.Id == request.SectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        var lesson = section.Lessons.FirstOrDefault(l => l.Id == request.LessonId);
        if (lesson is null)
            return Result.Failure(LessonErrors.NotFound);

        var material = lesson.Resources.FirstOrDefault(m => m.Id == request.MaterialId);
        if (material is null)
            return Result.Failure(LessonMaterialErrors.NotFound);

        await _storage.DeleteAsync(material.DriveFileId, cancellationToken);
        
        // Remove from db
        _context.Remove(material);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
