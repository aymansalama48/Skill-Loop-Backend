using FluentValidation;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyWalletTransactionsPaged;

public sealed class GetMyWalletTransactionsPagedQueryValidator : AbstractValidator<GetMyWalletTransactionsPagedQuery>
{
    public GetMyWalletTransactionsPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("رقم الصفحة يجب أن يكون أكبر من الصفر.");
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100).WithMessage("حجم الصفحة غير صالح.");
    }
}