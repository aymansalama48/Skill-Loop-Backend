using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.External.Payments;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.CreditPurchases.Common;
using Skill_Loop.Application.Features.CreditPurchases.DTOs;
using Skill_Loop.Application.Features.Promotions.Common;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Promotions;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.CreditPurchases.Commands.PurchaseCredits;

/// <summary>شحن المحفظة: اشترِ X كريديت (اختياري: بكود خصم).</summary>
public sealed record PurchaseCreditsCommand(Guid UserId, int Credits, string? PromoCode) : ICommand<CreditPurchaseDto>;

public sealed class PurchaseCreditsCommandValidator : AbstractValidator<PurchaseCreditsCommand>
{
    public PurchaseCreditsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Credits)
            .InclusiveBetween(CreditPurchase.MinCredits, CreditPurchase.MaxCredits)
            .WithMessage($"عدد الكريديت لازم يكون بين {CreditPurchase.MinCredits} و {CreditPurchase.MaxCredits}.");
    }
}

public sealed class PurchaseCreditsCommandHandler : ICommandHandler<PurchaseCreditsCommand, CreditPurchaseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGateway _gateway;
    private readonly ICreditPricing _pricing;

    public PurchaseCreditsCommandHandler(IApplicationDbContext context, IPaymentGateway gateway, ICreditPricing pricing)
    {
        _context = context;
        _gateway = gateway;
        _pricing = pricing;
    }

    public async Task<Result<CreditPurchaseDto>> Handle(PurchaseCreditsCommand request, CancellationToken cancellationToken)
    {
        // 1. كود الخصم (لو موجود): لازم صالح ومستخدمش قبل كده
        PromoCode? promo = null;
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoResult = await PromoCodeLookup.ResolveAsync(_context, request.PromoCode, request.UserId, cancellationToken);
            if (promoResult.IsFailure)
                return Result<CreditPurchaseDto>.Failure(promoResult.Errors);

            promo = promoResult.Data;
        }

        // 2. السعر النهائي (السيرفر بيحسبه بنفسه — ما بنثقش في أي رقم جاي من الموبايل)
        var quote = PurchaseQuoteCalculator.Calculate(request.Credits, _pricing, promo);

        // 3. سجّل طلب الشراء (Pending)
        var purchaseResult = CreditPurchase.Create(
            request.UserId, request.Credits, _pricing.Currency,
            quote.SubtotalMinor, quote.DiscountMinor, quote.TotalMinor, promo?.Id);

        if (purchaseResult.IsFailure)
            return Result<CreditPurchaseDto>.Failure(purchaseResult.Errors);

        var purchase = purchaseResult.Data!;

        // 4. الدفع. لو الإجمالي بقى صفر (خصم 100%) مفيش داعي نكلّم البوابة.
        PaymentSession session;
        if (quote.TotalMinor == 0)
        {
            session = new PaymentSession($"free-{purchase.Id}", null, IsCompleted: true);
        }
        else
        {
            var gatewayResult = await _gateway.CreatePaymentAsync(
                new PaymentRequest(purchase.Id, request.UserId, quote.TotalMinor, _pricing.Currency,
                    $"Purchase of {request.Credits} credits"),
                cancellationToken);

            if (gatewayResult.IsFailure)
                return Result<CreditPurchaseDto>.Failure(gatewayResult.Errors);

            session = gatewayResult.Data!;
        }

        _context.Add(purchase);

        // 5. الدفع تم فوراً؟ ضيف الكريديت للمحفظة + سجّل استخدام الكود، كله في Save واحد (Atomic)
        int? newBalance = null;

        if (session.IsCompleted)
        {
            var wallet = await _context.FirstOrDefaultAsync(
                _context.UserWallets.Where(w => w.UserId == request.UserId),
                cancellationToken);

            var isNewWallet = wallet is null;
            wallet ??= UserWallet.Create(request.UserId);

            var addResult = wallet.PurchaseCredits(
                request.Credits, purchase.Id, $"Purchased {request.Credits} credits");

            if (addResult.IsFailure)
                return Result<CreditPurchaseDto>.Failure(addResult.Errors);

            if (isNewWallet) _context.Add(wallet);
            else _context.Update(wallet); // نفس أسلوب EnrollInCourse

            purchase.MarkCompleted(session.GatewayReference);

            if (promo is not null)
            {
                var redeemResult = promo.Redeem(DateTime.UtcNow);
                if (redeemResult.IsFailure)
                    return Result<CreditPurchaseDto>.Failure(redeemResult.Errors);

                _context.Add(PromoRedemption.Create(promo.Id, request.UserId, purchase.Id, quote.DiscountMinor));
            }

            newBalance = wallet.Balance;
        }
        else
        {
            // بوابة حقيقية: المستخدم هيروح لصفحة الدفع، والكريديت بيتضاف لما البوابة تأكد (Webhook)
            purchase.SetGatewayReference(session.GatewayReference);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<CreditPurchaseDto>.Success(new CreditPurchaseDto(
            purchase.Id,
            purchase.Credits,
            purchase.Currency,
            purchase.SubtotalMinor,
            purchase.DiscountMinor,
            purchase.TotalMinor,
            purchase.Status.ToString(),
            session.CheckoutUrl,
            newBalance,
            purchase.CreatedAt));
    }
}
