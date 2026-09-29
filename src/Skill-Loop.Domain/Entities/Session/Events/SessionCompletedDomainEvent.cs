using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Entities.Sessions.Events;

// الحدث ده هيحتوي على كل البيانات اللي الهاندلرز ممكن تحتاجها
public sealed record SessionCompletedDomainEvent(
    Guid SessionId,
    Guid InstructorId, // مهم جداً عشان نزود عدد الجلسات في بروفايله
    Guid LearnerUserId,
    int PriceInCredits // مهم عشان محفظة المدرب لاحقاً
) : IDomainEvent;