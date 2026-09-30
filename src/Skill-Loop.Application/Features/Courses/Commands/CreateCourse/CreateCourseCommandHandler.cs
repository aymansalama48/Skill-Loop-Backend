using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Category;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage; // responsible for storage setup
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
private readonly ICurrentUser _currentUser;
    private readonly IFileStorage _fileStorage; // real storage dependency

    public CreateCourseCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IFileStorage fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        // 1. التأكد من وجود القسم
        var categoryExists = await _context.AnyAsync(
            _context.Categories.Where(c => c.Id == request.CategoryId),
            cancellationToken);

        if (!categoryExists)
        {
            return Result<Guid>.Failure(CategoryErrors.NotFound);
        }

// Merge note: this handler gained two independent features from opposite sides and needs
// both, because Course.Create consumes all three values. Order matters for security: the
// instructor identity is resolved first so a caller who is not allowed to publish as their
// claimed instructor never causes a file upload as a side effect.
var canManageAll = _currentUser.HasPermission(Permissions.Courses.ManageAll);

var instructorId = canManageAll ? request.InstructorId : _currentUser.UserId!.Value;
var instructorName = canManageAll
    ? request.InstructorName
    : _currentUser.FullName ?? string.Empty;

// 2. Thumbnail upload, when one was supplied.
string uploadedThumbnailUrl = string.Empty;

if (request.ThumbnailStream is not null && !string.IsNullOrWhiteSpace(request.ThumbnailFileName))
{
    // The "Courses" parameter determines the storage folder.
    var uploadResult = await _fileStorage.UploadAsync(
        request.ThumbnailStream,
        request.ThumbnailFileName,
        "Courses");

    // If the upload fails, abort with the same error and do not proceed.
    if (uploadResult.IsFailure)
    {
        return Result<Guid>.Failure(uploadResult.Errors.First());
    }

    // A successful upload yields the relative file path.
    uploadedThumbnailUrl = uploadResult.Data!;
}

// 3. Course persistence (zero or one thumbnail)
        var courseResult = Course.Create(
            request.Title,
            request.Description,
            uploadedThumbnailUrl,
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