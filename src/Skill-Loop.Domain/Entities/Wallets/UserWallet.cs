using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Wallets.Events;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Wallets;

public sealed class UserWallet : AuditableEntity
{
    public Guid UserId { get; private set; }
    public int Balance { get; private set; }
    public byte[] RowVersion { get; private set; } = []; // Optimistic Concurrency Token

    private readonly List<WalletTransaction> _transactions = [];
    public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private UserWallet() { }

    public static UserWallet Create(Guid userId, int initialBalance = 0)
    {
        return new UserWallet
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Balance = Math.Max(0, initialBalance)
        };
    }

    public Result AddCredits(int amount, Guid referenceId, string description)
    {
        if (amount <= 0)
            return Result.Failure(new Error("Wallet.InvalidAmount", "Credit amount must be positive.", ErrorType.Validation));

        Balance += amount;
        _transactions.Add(WalletTransaction.Create(Id, amount, TransactionType.CreditReward, referenceId, description));

        return Result.Success();
    }

    public Result DeductCredits(int amount, Guid referenceId, string description)
    {
        if (amount <= 0)
            return Result.Failure(new Error("Wallet.InvalidAmount", "Credit amount must be positive.", ErrorType.Validation));

        if (Balance < amount)
            return Result.Failure(new Error("Wallet.InsufficientBalance", $"Insufficient credits. Required: {amount}, Available: {Balance}", ErrorType.Failure));

        Balance -= amount;
        _transactions.Add(WalletTransaction.Create(Id, amount, TransactionType.CreditDeduction, referenceId, description));

        AddDomainEvent(new WalletBalanceDeductedDomainEvent(UserId, amount, Balance));
        return Result.Success();
    }

    /// <summary>
    /// استرجاع credits (إلغاء حجز / استرجاع purchase) — بيتسجل كـ CreditRefund
    /// عشان الـ transaction history يبقى واضح.
    /// </summary>
    public Result RefundCredits(int amount, Guid referenceId, string description)
    {
        if (amount <= 0)
            return Result.Failure(new Error("Wallet.InvalidAmount", "Refund amount must be positive.", ErrorType.Validation));

        Balance += amount;
        _transactions.Add(WalletTransaction.Create(Id, amount, TransactionType.CreditRefund, referenceId, description));

        AddDomainEvent(new WalletBalanceRefundedDomainEvent(UserId, amount, Balance));
        return Result.Success();
    }
}
