using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterialDownloadInfo;

public sealed class GetSessionMaterialDownloadInfoQueryValidator : AbstractValidator<GetSessionMaterialDownloadInfoQuery>
{
    public GetSessionMaterialDownloadInfoQueryValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Session ID.");

        RuleFor(x => x.MaterialId)
            .NotEmpty().WithMessage("Material ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Material ID.");
    }
}