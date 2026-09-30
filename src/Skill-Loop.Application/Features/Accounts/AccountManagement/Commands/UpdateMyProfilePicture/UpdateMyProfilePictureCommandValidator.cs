using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;

public sealed class UpdateMyProfilePictureCommandValidator : AbstractValidator<UpdateMyProfilePictureCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public UpdateMyProfilePictureCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("This field is required.")
            .Must(s => s != null && s.Length > 0).WithMessage("Invalid value.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("This field is required.")
            .Must(fileName => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
            .WithMessage("Invalid value.");
    }
}