namespace Skill_Loop.Domain.Common.Events;

// "عقد": أي كلاس عايز يخزن أخبار، لازم يقدر يعمل الحاجتين دول
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}