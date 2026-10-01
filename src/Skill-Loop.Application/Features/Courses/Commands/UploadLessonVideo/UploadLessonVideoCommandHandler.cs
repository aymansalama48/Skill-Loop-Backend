using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Course;
using Skill_Loop.Domain.Common.Results;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Skill_Loop.Application.Features.Courses.Commands.UploadLessonVideo;

public sealed class UploadLessonVideoCommandHandler : ICommandHandler<UploadLessonVideoCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly Skill_Loop.Application.Common.Abstractions.External.Media.IVideoAnalyzer _videoAnalyzer;

    public UploadLessonVideoCommandHandler(
        IApplicationDbContext context, 
        IFileStorage fileStorage,
        Skill_Loop.Application.Common.Abstractions.External.Media.IVideoAnalyzer videoAnalyzer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _videoAnalyzer = videoAnalyzer;
    }

    public async Task<Result> Handle(UploadLessonVideoCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
            return Result.Failure(CourseErrors.NotFound);

        var section = course.Sections.FirstOrDefault(s => s.Id == request.SectionId);
        var lesson = section?.Lessons.FirstOrDefault(l => l.Id == request.LessonId);

        if (lesson is null)
            return Result.Failure(CourseErrors.NotFound); // Or LessonErrors.NotFound

        var uploadResult = await _fileStorage.UploadAsync(
            request.VideoStream,
            request.VideoFileName,
            $"courses/{request.CourseId}/lessons");

        if (uploadResult.IsFailure)
        {
            return Result.Failure(uploadResult.Errors.First());
        }

        string videoUrl = uploadResult.Data!;

        var analysisResult = await _videoAnalyzer.AnalyzeVideoAsync(videoUrl, cancellationToken);
        if (analysisResult.IsFailure)
        {
            return Result.Failure(analysisResult.Errors.First());
        }

        var duration = analysisResult.Data!.Duration;
        var resolution = analysisResult.Data.Resolution;

        var updateResult = course.UpdateLessonVideo(request.SectionId, request.LessonId, videoUrl, duration, resolution, null);

        if (updateResult.IsFailure)
            return updateResult;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
