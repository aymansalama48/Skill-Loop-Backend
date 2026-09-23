using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.DeleteSessionMaterial;

public sealed class DeleteSessionMaterialCommandValidator : AbstractValidator<DeleteSessionMaterialCommand>
{
    public DeleteSessionMaterialCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Session ID.");

        RuleFor(x => x.MaterialId)
            .NotEmpty().WithMessage("Material ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Material ID.");
    }
}