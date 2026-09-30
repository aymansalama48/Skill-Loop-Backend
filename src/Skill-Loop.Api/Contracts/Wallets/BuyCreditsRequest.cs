namespace Skill_Loop.Api.Contracts.Wallets;

public sealed record BuyCreditsRequest(
    int Amount,
    string? PromoCode);
