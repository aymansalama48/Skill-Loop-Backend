using Skill_Loop.Application.Common.Errors.Course;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;


namespace Skill_Loop.Application.Features.Courses.Commands.AddLesson;

public sealed class AddLessonCommandHandler : ICommandHandler<AddLessonCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddLessonCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddLessonCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result<Guid>.Failure(CourseErrors.NotFound);
        }

        var addLessonResult = course.AddLessonToSection(
            request.SectionId,
            request.Title,
            request.VideoUrl,
            request.Duration,
            request.StreamingResolution ?? "1080p",
            request.ExternalProviderId,
            request.OrderIndex,
            request.IsPreviewable);

        if (addLessonResult.IsFailure)
        {
            return Result<Guid>.Failure(addLessonResult.Errors.First());
        }

        await _context.SaveChangesAsync(cancellationToken);

        var addedLesson = course.Sections
            .First(s => s.Id == request.SectionId)
            .Lessons
            .Last();

        return Result<Guid>.Success(addedLesson.Id);
    }
}