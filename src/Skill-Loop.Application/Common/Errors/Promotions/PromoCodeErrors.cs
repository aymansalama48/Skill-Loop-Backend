using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Promotions;

public static class PromoCodeErrors
{
    public static readonly Error NotFound = new(
        "PROMO_NOT_FOUND", "كود الخصم غير صحيح.", ErrorType.NotFound);

    public static readonly Error DuplicateCode = new(
        "PROMO_DUPLICATE_CODE", "الكود ده موجود بالفعل.", ErrorType.Conflict);

    public static readonly Error AlreadyUsed = new(
        "PROMO_ALREADY_USED", "لقد استخدمت كود الخصم ده من قبل.", ErrorType.Conflict);

    public static readonly Error Forbidden = new(
        "PROMO_FORBIDDEN", "ليس لديك صلاحية إدارة أكواد الخصم.", ErrorType.Forbidden);
}
