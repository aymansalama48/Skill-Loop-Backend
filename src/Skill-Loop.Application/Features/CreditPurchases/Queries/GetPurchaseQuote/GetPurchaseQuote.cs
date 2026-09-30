using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Payments;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.CreditPurchases.Common;
using Skill_Loop.Application.Features.CreditPurchases.DTOs;
using Skill_Loop.Application.Features.Promotions.Common;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Promotions;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.CreditPurchases.Queries.GetPurchaseQuote;

/// <summary>
/// "هدفع كام لو اشتريت X كريديت (وبالكود ده)؟" — معاينة بس، مفيش أي حاجة بتتحفظ.
/// شاشة الدفع بتناديها لما المستخدم يكتب كود الخصم عشان يشوف السعر الجديد.
/// </summary>
[AuthenticatedOnly]
public sealed record GetPurchaseQuoteQuery(Guid UserId, int Credits, string? PromoCode) : IQuery<PurchaseQuoteDto>;

public sealed class GetPurchaseQuoteQueryValidator : AbstractValidator<GetPurchaseQuoteQuery>
{
    public GetPurchaseQuoteQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Credits)
            .InclusiveBetween(CreditPurchase.MinCredits, CreditPurchase.MaxCredits)
            .WithMessage($"عدد الكريديت لازم يكون بين {CreditPurchase.MinCredits} و {CreditPurchase.MaxCredits}.");
    }
}

public sealed class GetPurchaseQuoteQueryHandler : IQueryHandler<GetPurchaseQuoteQuery, PurchaseQuoteDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICreditPricing _pricing;

    public GetPurchaseQuoteQueryHandler(IApplicationDbContext context, ICreditPricing pricing)
    {
        _context = context;
        _pricing = pricing;
    }

    public async Task<Result<PurchaseQuoteDto>> Handle(GetPurchaseQuoteQuery request, CancellationToken cancellationToken)
    {
        PromoCode? promo = null;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoResult = await PromoCodeLookup.ResolveAsync(_context, request.PromoCode, request.UserId, cancellationToken);
            if (promoResult.IsFailure)
                return Result<PurchaseQuoteDto>.Failure(promoResult.Errors);

            promo = promoResult.Data;
        }

        var quote = PurchaseQuoteCalculator.Calculate(request.Credits, _pricing, promo);

        return Result<PurchaseQuoteDto>.Success(new PurchaseQuoteDto(
            request.Credits,
            _pricing.Currency,
            quote.SubtotalMinor,
            quote.DiscountMinor,
            quote.TotalMinor,
            promo?.Code));
    }
}
