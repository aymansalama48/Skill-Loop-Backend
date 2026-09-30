using Skill_Loop.Application.Common.Errors.CourseMaterial;
using Skill_Loop.Application.Common.Errors.Course;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Commands.RemoveCourseMaterial;

public sealed class RemoveCourseMaterialCommandHandler : ICommandHandler<RemoveCourseMaterialCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICourseContentStorage _storage;

    public RemoveCourseMaterialCommandHandler(IApplicationDbContext context, ICourseContentStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Result> Handle(RemoveCourseMaterialCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Include(c => c.Attachments).Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
            return Result.Failure(CourseErrors.NotFound);

        var material = course.Attachments.FirstOrDefault(m => m.Id == request.MaterialId);
        if (material is null)
            return Result.Failure(CourseMaterialErrors.NotFound);

        await _storage.DeleteAsync(material.DriveFileId, cancellationToken);
        
        // Remove from db
        _context.Remove(material);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
