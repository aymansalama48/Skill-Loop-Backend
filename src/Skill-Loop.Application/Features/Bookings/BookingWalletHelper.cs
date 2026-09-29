using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.Bookings;

/// <summary>
/// استرجاع الـ credits للمتعلم لما الحجز يتلغى أو يترفض.
/// (المحفظة متتبَّعة أصلاً — فبنكتفي بالتغييرات على الـ change tracker من غير Update يدوي)
/// </summary>
public static class BookingWalletHelper
{
    public static async Task<Result> RefundAsync(
        IApplicationDbContext dbContext,
        Booking booking,
        string sessionTitle,
        CancellationToken cancellationToken)
    {
        if (booking.PriceInCredits <= 0)
        {
            return Result.Success();
        }

        var wallet = await dbContext.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == booking.LearnerUserId, cancellationToken);

        // (Lazy Creation) لو المتعلم معندوش محفظة لسه
        if (wallet is null)
        {
            wallet = UserWallet.Create(booking.LearnerUserId, 0);
            dbContext.Add(wallet);
        }

        return wallet.RefundCredits(
            booking.PriceInCredits,
            booking.Id,
            $"Refund for cancelled booking of: {sessionTitle}");
    }
}
