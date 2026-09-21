using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Common.Entities;

/// <summary>
/// الكيان الأساسي الذي يدعم أحداث المجال (Domain Events)
/// </summary>
public abstract class Entity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}