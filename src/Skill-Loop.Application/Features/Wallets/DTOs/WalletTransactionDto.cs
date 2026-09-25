namespace Skill_Loop.Application.Features.Wallets.DTOs;

public sealed record WalletTransactionDto(
    Guid TransactionId,
    int Amount,
    string Type, // "CreditReward" أو "CreditDeduction"
    string Description,
    DateTime OccurredAt
);