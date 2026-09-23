using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;

public sealed class UpdateMyProfilePictureCommandValidator : AbstractValidator<UpdateMyProfilePictureCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public UpdateMyProfilePictureCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("الملف مطلوب.")
            .Must(s => s != null && s.Length > 0).WithMessage("الملف غير صالح أو فارغ.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("اسم الملف مطلوب.")
            .Must(fileName => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
            .WithMessage("نوع الملف غير مدعوم. الصيغ المدعومة هي: jpg, jpeg, png, webp");
    }
}