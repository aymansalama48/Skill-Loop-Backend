using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Enrollments.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Enrollments;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Enrollments.Commands.EnrollInCourse;

public sealed record EnrollInCourseCommand(Guid UserId, Guid CourseId) : ICommand<EnrollmentResultDto>;

public sealed class EnrollInCourseCommandValidator : AbstractValidator<EnrollInCourseCommand>
{
    public EnrollInCourseCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.CourseId).NotEmpty().WithMessage("Course ID is required.");
    }
}

public sealed class EnrollInCourseCommandHandler : ICommandHandler<EnrollInCourseCommand, EnrollmentResultDto>
{
    private readonly IApplicationDbContext _context;

    public EnrollInCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EnrollmentResultDto>> Handle(EnrollInCourseCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch Course details
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Where(c => c.Id == request.CourseId && c.Status == CourseStatus.Published && !c.IsDeleted),
            cancellationToken);

        if (course is null)
        {
            return Result<EnrollmentResultDto>.Failure(new Error("Course.NotFound", "Course does not exist or is not published.", ErrorType.NotFound));
        }

        // 2. Prevent duplicate active enrollments (Idempotency)
        var isAlreadyEnrolled = await _context.AnyAsync(
            _context.Enrollments.Where(e => e.UserId == request.UserId && e.CourseId == request.CourseId),
            cancellationToken);

        if (isAlreadyEnrolled)
        {
            return Result<EnrollmentResultDto>.Failure(new Error("Enrollment.Duplicate", "User is already enrolled in this course.", ErrorType.Conflict));
        }

        // 3. Virtual Wallet balance validation & atomic deduction
        if (!course.Price.IsFree)
        {
            var wallet = await _context.FirstOrDefaultAsync(
                _context.UserWallets.Where(w => w.UserId == request.UserId),
                cancellationToken);

            if (wallet is null)
            {
                return Result<EnrollmentResultDto>.Failure(new Error("Wallet.NotFound", "User virtual wallet not found.", ErrorType.NotFound));
            }

            var deductionResult = wallet.DeductCredits(
                course.Price.Credits,
                course.Id,
                $"Enrolled in course: {course.Title}");

            if (deductionResult.IsFailure)
            {
                return Result<EnrollmentResultDto>.Failure(deductionResult.Errors.First());
            }

            _context.Update(wallet);
        }

        // 4. Create Enrollment Record
        var enrollmentResult = Enrollment.Create(
            request.UserId,
            course.Id,
            course.Price.Credits,
            course.TotalLessonsCount);

        if (enrollmentResult.IsFailure)
        {
            return Result<EnrollmentResultDto>.Failure(enrollmentResult.Errors.First());
        }

        var enrollment = enrollmentResult.Data!;
        _context.Add(enrollment);

        // 5. Commit atomic transaction (Outbox interceptor records domain events)
        await _context.SaveChangesAsync(cancellationToken);

        return Result<EnrollmentResultDto>.Success(new EnrollmentResultDto(
            enrollment.Id,
            enrollment.UserId,
            enrollment.CourseId,
            enrollment.CreditsPaid,
            enrollment.Status.ToString(),
            enrollment.EnrolledAt));
    }
}
