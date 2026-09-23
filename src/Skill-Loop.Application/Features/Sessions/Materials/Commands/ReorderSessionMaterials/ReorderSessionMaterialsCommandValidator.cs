using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.ReorderSessionMaterials;

public sealed class ReorderSessionMaterialsCommandValidator : AbstractValidator<ReorderSessionMaterialsCommand>
{
    public ReorderSessionMaterialsCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Session ID.");

        RuleFor(x => x.OrderedMaterialIds)
            .NotNull().WithMessage("Ordered material IDs list is required.")
            .NotEmpty().WithMessage("Ordered material IDs list cannot be empty.")
            .Must(ids => ids.Count == ids.Distinct().Count())
                .WithMessage("Material IDs must be unique.")
            .ForEach(id => id
                .NotEqual(Guid.Empty).WithMessage("Material ID cannot be empty."));
    }
}