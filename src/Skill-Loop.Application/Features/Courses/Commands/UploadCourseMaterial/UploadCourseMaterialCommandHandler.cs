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

namespace Skill_Loop.Application.Features.Courses.Commands.UploadCourseMaterial;

public sealed class UploadCourseMaterialCommandHandler : ICommandHandler<UploadCourseMaterialCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICourseContentStorage _storage;
    private readonly ICurrentUser _currentUser;

    public UploadCourseMaterialCommandHandler(
        IApplicationDbContext context, 
        ICourseContentStorage storage, 
        ICurrentUser currentUser)
    {
        _context = context;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(UploadCourseMaterialCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Include(c => c.Attachments).Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
            return Result<Guid>.Failure(CourseErrors.NotFound);

        var folderName = $"course-{request.CourseId}";
        var uploadResult = await _storage.UploadAsync(request.FileStream, request.FileName, folderName, cancellationToken);
        if (!uploadResult.IsSuccess)
            return Result<Guid>.Failure(uploadResult.Errors);

        var uploadData = uploadResult.Data;
        var sortOrder = course.Attachments.Any() ? course.Attachments.Max(m => m.SortOrder) + 1 : 0;
        
        var material = CourseMaterial.Create(
            request.CourseId,
            request.FileName,
            request.MimeType,
            request.SizeBytes,
            uploadData.DriveFileId,
            uploadData.DriveFolderId,
            sortOrder,
            _currentUser.UserId ?? Guid.Empty,
            request.MaterialType);

        var addResult = course.AddAttachment(material);
        if (addResult.IsFailure)
            return Result<Guid>.Failure(addResult.Errors);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(material.Id);
    }
}
