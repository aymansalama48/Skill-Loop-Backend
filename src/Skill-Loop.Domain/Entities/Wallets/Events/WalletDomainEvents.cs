using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Entities.Wallets.Events;

public sealed record WalletBalanceDeductedDomainEvent(
    Guid UserId,
    int AmountDeducted,
    int RemainingBalance) : IDomainEvent;

public sealed record WalletBalanceRefundedDomainEvent(
    Guid UserId,
    int AmountRefunded,
    int RemainingBalance) : IDomainEvent;
