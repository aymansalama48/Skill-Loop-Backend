using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.Domain.Entities.Courses.ValueObjects;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

public sealed record CreateCourseCommand(
    string Title,
    string Description,
    string ThumbnailUrl,
    int Credits,
    CourseLevel Level,
    Guid InstructorId,
    string InstructorName,
    Guid CategoryId) : ICommand<Guid>;

public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.");

        RuleFor(x => x.ThumbnailUrl)
            .NotEmpty().WithMessage("Thumbnail URL is required.");

        RuleFor(x => x.Credits)
            .GreaterThanOrEqualTo(0).WithMessage("Credits cannot be negative.");

        RuleFor(x => x.InstructorId)
            .NotEmpty().WithMessage("Instructor is required.");

        RuleFor(x => x.InstructorName)
            .NotEmpty().WithMessage("Instructor name is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");
    }
}

public sealed class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.AnyAsync(
            _context.Categories.Where(c => c.Id == request.CategoryId),
            cancellationToken);

        if (!categoryExists)
        {
            return Result<Guid>.Failure(new Error("Category.NotFound", "The specified category was not found.", ErrorType.NotFound));
        }

        var priceResult = CoursePrice.Create(request.Credits);
        if (priceResult.IsFailure)
        {
            return Result<Guid>.Failure(priceResult.Errors.First());
        }

        var courseResult = Course.Create(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            priceResult.Data!,
            request.Level,
            request.InstructorId,
            request.InstructorName,
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
