using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Sessions.Events;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.Wallets.EventHandlers.SessionCompleted;

/// <summary>
/// أول ما الجلسةخلص فعلاً، الـ credits بتروح لمحفظة المحاضر.
/// (كان ناقص في الإصدار السابق — كان بيتحسب Earnings في البروفايل بس من غير رصيد فعلي)
/// </summary>
public sealed class CreditInstructorWalletOnSessionCompletedEventHandler(
    IApplicationDbContext _dbContext) : INotificationHandler<DomainEventNotification<SessionCompletedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<SessionCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // 1. جلسة مجانية (أو سعر صفر) → مفيش أرباح
        if (domainEvent.PriceInCredits <= 0) return;

        // 2. جلب الجلسة لمعرفة العنوان (نفسه اللي بنحطه في وصف الحركة)
        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == domainEvent.SessionId, cancellationToken);

        if (session is null) return;

        // 3. جلب محفظة المحاضر (Lazy Creation)
        var wallet = await _dbContext.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == domainEvent.InstructorId, cancellationToken);

        if (wallet is null)
        {
            wallet = UserWallet.Create(domainEvent.InstructorId, 0);
            _dbContext.Add(wallet);
        }

        // 4. إضافة الأرباح — الـ BookingId هو الـ ReferenceId
        var description = $"أرباح جلسة لايف: {session.Title}";

        var addResult = wallet.AddCredits(
            domainEvent.PriceInCredits,
            domainEvent.SessionId,
            description);

        if (addResult.IsFailure) return;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
