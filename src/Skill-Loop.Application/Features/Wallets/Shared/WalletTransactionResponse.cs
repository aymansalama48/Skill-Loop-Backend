namespace Skill_Loop.Application.Features.Wallets.Shared;

public sealed record WalletTransactionResponse(
    Guid TransactionId,
    int Amount,
    string Type, // "CreditReward" أو "CreditDeduction"
    string Description,
    DateTime OccurredAt
);