using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Wallet;

public static class WalletErrors
{
    public static readonly Error InvalidAmount = new Error(
        "Wallet.InvalidAmount",
        "Wallet invalid amount.",
        ErrorType.Validation);

}
