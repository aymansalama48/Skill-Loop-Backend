using Skill_Loop.Application.Common.Errors.Lesson;
using Skill_Loop.Application.Common.Errors.Section;
using Skill_Loop.Application.Common.Errors.Course;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Commands.UploadLessonMaterial;

public sealed class UploadLessonMaterialCommandHandler : ICommandHandler<UploadLessonMaterialCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICourseContentStorage _storage;
    private readonly ICurrentUser _currentUser;

    public UploadLessonMaterialCommandHandler(
        IApplicationDbContext context, 
        ICourseContentStorage storage, 
        ICurrentUser currentUser)
    {
        _context = context;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(UploadLessonMaterialCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .ThenInclude(l => l.Resources)
                .Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
            return Result<Guid>.Failure(CourseErrors.NotFound);

        var section = course.Sections.FirstOrDefault(s => s.Id == request.SectionId);
        if (section is null)
            return Result<Guid>.Failure(SectionErrors.NotFound);

        var lesson = section.Lessons.FirstOrDefault(l => l.Id == request.LessonId);
        if (lesson is null)
            return Result<Guid>.Failure(LessonErrors.NotFound);

        var folderName = $"lesson-{request.LessonId}";
        var uploadResult = await _storage.UploadAsync(request.FileStream, request.FileName, folderName, cancellationToken);
        if (!uploadResult.IsSuccess)
            return Result<Guid>.Failure(uploadResult.Errors);

        var uploadData = uploadResult.Data;
        var sortOrder = lesson.Resources.Any() ? lesson.Resources.Max(m => m.SortOrder) + 1 : 0;
        
        var material = LessonMaterial.Create(
            request.LessonId,
            request.FileName,
            request.MimeType,
            request.SizeBytes,
            uploadData.DriveFileId,
            uploadData.DriveFolderId,
            sortOrder,
            _currentUser.UserId ?? Guid.Empty,
            request.MaterialType);

        lesson.AddResource(material);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(material.Id);
    }
}
