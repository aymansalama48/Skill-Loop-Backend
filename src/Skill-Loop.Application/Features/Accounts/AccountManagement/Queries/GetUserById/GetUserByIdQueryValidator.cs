using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetUserById;

public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("This field is required.");
    }
}