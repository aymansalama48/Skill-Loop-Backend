namespace Skill_Loop.Domain.Enums;

public enum BookingStatus
{
    /// <summary>في انتظار موافقة المحاضر (لو الجلسة بتطلب تأكيد)</summary>
    Pending = 1,

    /// <summary>مؤكدة وال credits اتخصمت من/learner</summary>
    Confirmed = 2,

    /// <summary>الجلسة بدأت فعلياً</summary>
    InProgress = 3,

    /// <summary>خلصت و credits اتحوّلت للمحاضر</summary>
    Completed = 4,

    /// <summary>اتلغت من المتعلم أو المحاضر (مع استرجاع credits)</summary>
    Cancelled = 5,

    /// <summary>المحاضر رفض الحجز (مع استرجاع credits)</summary>
    Rejected = 6,

    /// <summary>المتعلم لم يحضر (من دون استرجاع)</summary>
    NoShow = 7
}
