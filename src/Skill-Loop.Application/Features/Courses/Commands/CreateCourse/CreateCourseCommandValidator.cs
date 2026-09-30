using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

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

        // 1. التحقق من مساحة الملف (إذا تم إرفاقه)
        RuleFor(x => x.ThumbnailStream)
            .Must(stream => stream!.Length > 0)
            .When(x => x.ThumbnailStream != null) // يطبق الشرط فقط لو اليوزر رفع صورة
            .WithMessage("Thumbnail image cannot be empty.");

        // 2. التحقق من وجود اسم للملف (إذا تم إرفاق ملف)
        RuleFor(x => x.ThumbnailFileName)
            .NotEmpty().WithMessage("Thumbnail file name is required.")
            .When(x => x.ThumbnailStream != null);

        RuleFor(x => x.Credits)
            .GreaterThanOrEqualTo(0).WithMessage("Credits cannot be negative.");

        RuleFor(x => x.InstructorId)
            .NotEmpty().WithMessage("Instructor is required.")
            .NotEqual(Guid.Empty).WithMessage("Instructor ID is invalid.");

        RuleFor(x => x.InstructorName)
            .NotEmpty().WithMessage("Instructor name is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.")
            .NotEqual(Guid.Empty).WithMessage("Category ID is invalid.");
    }
}