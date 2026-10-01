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
    private readonly IFileStorage _fileStorage;

    public UpdateLessonCommandHandler(IApplicationDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
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

        var section = course.Sections.FirstOrDefault(s => s.Id == request.SectionId);
        var lesson = section?.Lessons.FirstOrDefault(l => l.Id == request.LessonId);
        string videoUrl = lesson?.VideoUrl ?? string.Empty;

        if (request.VideoStream is not null && !string.IsNullOrWhiteSpace(request.VideoFileName))
        {
            var uploadResult = await _fileStorage.UploadAsync(
                request.VideoStream,
                request.VideoFileName,
                $"courses/{request.CourseId}/lessons");
            
            if (uploadResult.IsFailure)
                return Result.Failure(uploadResult.Errors.First());
            
            videoUrl = uploadResult.Data;
        }

        var result = course.UpdateLesson(
            request.SectionId, 
            request.LessonId, 
            request.Title, 
            videoUrl, 
            request.Duration, 
            request.StreamingResolution, 
            request.ExternalProviderId, 
            request.OrderIndex, 
            request.IsPreviewable);

        if (result.IsFailure) return result;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
