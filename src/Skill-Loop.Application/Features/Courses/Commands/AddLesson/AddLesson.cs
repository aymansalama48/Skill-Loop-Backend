using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses.ValueObjects;

namespace Skill_Loop.Application.Features.Courses.Commands.AddLesson;

public sealed record AddLessonCommand(
    Guid CourseId,
    Guid SectionId,
    string Title,
    string VideoUrl,
    TimeSpan Duration,
    int OrderIndex,
    bool IsPreviewable,
    string? StreamingResolution,
    string? ExternalProviderId) : ICommand<Guid>;

public sealed class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty().WithMessage("Course ID is required.");
        RuleFor(x => x.SectionId).NotEmpty().WithMessage("Section ID is required.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.VideoUrl).NotEmpty();
        RuleFor(x => x.Duration).GreaterThan(TimeSpan.Zero).WithMessage("Duration must be greater than zero.");
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}

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
            return Result<Guid>.Failure(new Error("Course.NotFound", "Course was not found.", ErrorType.NotFound));
        }

        var videoResult = VideoResource.Create(
            request.VideoUrl,
            request.Duration,
            request.StreamingResolution ?? "1080p",
            request.ExternalProviderId);

        if (videoResult.IsFailure)
        {
            return Result<Guid>.Failure(videoResult.Errors.First());
        }

        var addLessonResult = course.AddLessonToSection(
            request.SectionId,
            request.Title,
            videoResult.Data!,
            request.OrderIndex,
            request.IsPreviewable);

        if (addLessonResult.IsFailure)
        {
            return Result<Guid>.Failure(addLessonResult.Errors.First());
        }

        _context.Update(course);
        await _context.SaveChangesAsync(cancellationToken);

        var addedLesson = course.Sections
            .First(s => s.Id == request.SectionId)
            .Lessons
            .Last();

        return Result<Guid>.Success(addedLesson.Id);
    }
}
