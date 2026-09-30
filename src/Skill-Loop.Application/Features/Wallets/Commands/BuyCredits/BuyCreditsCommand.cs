using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Wallets.Commands.BuyCredits;

[AuthenticatedOnly]
public record BuyCreditsCommand(int Amount, string? PromoCode) : IRequest<Result<bool>>;

public class BuyCreditsCommandValidator : AbstractValidator<BuyCreditsCommand>
{
    public BuyCreditsCommandValidator()
    {
        RuleFor(v => v.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }
}

public class BuyCreditsCommandHandler : IRequestHandler<BuyCreditsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public BuyCreditsCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(BuyCreditsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
        
        var wallet = await _context.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            wallet = Skill_Loop.Domain.Entities.Wallets.UserWallet.Create(userId);
            _context.Add(wallet);
        }

        decimal discount = 0;
        if (!string.IsNullOrEmpty(request.PromoCode))
        {
            var promo = await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code == request.PromoCode.ToUpper(), cancellationToken);

            if (promo == null)
                return Result<bool>.Failure("Invalid promo code.");

            var checkResult = promo.CheckUsable(DateTime.UtcNow);
            if (checkResult.IsFailure)
                return Result<bool>.Failure(checkResult.Errors.First().Description);

            var redeemResult = promo.Redeem(DateTime.UtcNow);
            if (redeemResult.IsFailure)
                return Result<bool>.Failure(redeemResult.Errors.First().Description);

            // discount logic would need to calculate price of credits, but for now just note it
            discount = promo.DiscountType == Skill_Loop.Domain.Enums.DiscountType.Percentage ? promo.DiscountValue : 0;
        }

        // 1 Credit = 1 USD for example.
        // Fake checkout: we just add the credits directly.
        
        var walletResult = wallet.AddCredits(request.Amount, Guid.NewGuid(), $"Purchased {request.Amount} credits" + (discount > 0 ? $" with {discount}% discount" : ""));

        if (!walletResult.IsSuccess)
            return Result<bool>.Failure("Failed to add credits");
        
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<bool>.Failure("Concurrency conflict occurred. Please try again.");
        }

        return Result<bool>.Success(true);
    }
}
