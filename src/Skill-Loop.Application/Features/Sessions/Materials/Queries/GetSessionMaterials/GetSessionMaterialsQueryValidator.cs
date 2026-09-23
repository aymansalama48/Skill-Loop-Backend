using FluentValidation;
using Skill_Loop.Application.Common.Pagination;

namespace Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterials;

public sealed class GetSessionMaterialsQueryValidator : AbstractValidator<GetSessionMaterialsQuery>
{
    public GetSessionMaterialsQueryValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Session ID.");

        RuleFor(x => x.Pagination)
            .NotNull().WithMessage("Pagination parameters are required.");

        RuleFor(x => x.Pagination.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than zero.");

        RuleFor(x => x.Pagination.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
    }
}