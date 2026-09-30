using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Enrollments.Commands.UpdateLessonProgress;

public sealed record UpdateLessonProgressCommand(
    Guid UserId,
    Guid CourseId,
    Guid LessonId) : ICommand<double>;

public sealed class UpdateLessonProgressCommandValidator : AbstractValidator<UpdateLessonProgressCommand>
{
    public UpdateLessonProgressCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
    }
}

public sealed class UpdateLessonProgressCommandHandler : ICommandHandler<UpdateLessonProgressCommand, double>
{
    private readonly IApplicationDbContext _context;

    public UpdateLessonProgressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<double>> Handle(UpdateLessonProgressCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.FirstOrDefaultAsync(
            _context.Enrollments
                .Include(e => e.LessonProgresses)
                .Where(e => e.UserId == request.UserId && e.CourseId == request.CourseId),
            cancellationToken);

        if (enrollment is null)
        {
            return Result<double>.Failure(new Error("Enrollment.NotFound", "Enrollment record was not found.", ErrorType.NotFound));
        }

        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Where(c => c.Id == request.CourseId),
            cancellationToken);

        var totalLessons = course?.TotalLessonsCount ?? 1;

        var progressResult = enrollment.MarkLessonCompleted(request.LessonId, totalLessons);
        if (progressResult.IsFailure)
        {
            return Result<double>.Failure(progressResult.Errors.First());
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<double>.Success(enrollment.ProgressPercentage);
    }
}
