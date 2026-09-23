using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Wallets;

public sealed class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; private set; }
    public int Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public Guid ReferenceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    private WalletTransaction() { }

    internal static WalletTransaction Create(
        Guid walletId,
        int amount,
        TransactionType type,
        Guid referenceId,
        string description) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            WalletId = walletId,
            Amount = amount,
            Type = type,
            ReferenceId = referenceId,
            Description = description.Trim(),
            OccurredAt = DateTime.UtcNow
        };
}
