namespace Skill_Loop.Domain.Enums;

public enum CreditPurchaseStatus
{
    /// <summary>اتعمل الطلب ومستنيين تأكيد الدفع من البوابة.</summary>
    Pending = 1,

    /// <summary>الدفع تم ✅ والكريديت اتضاف للمحفظة.</summary>
    Completed = 2,

    Failed = 3
}
