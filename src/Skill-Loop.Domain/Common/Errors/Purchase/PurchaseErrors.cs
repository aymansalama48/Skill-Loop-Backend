using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Purchase;

public static class PurchaseErrors
{
    public static readonly Error InvalidUser = new Error(
        "Purchase.InvalidUser",
        "Purchase is required.",
        ErrorType.Validation);

    public static readonly Error InvalidAmounts = new Error(
        "Purchase.InvalidAmounts",
        "Purchase invalid amounts.",
        ErrorType.Validation);

}
