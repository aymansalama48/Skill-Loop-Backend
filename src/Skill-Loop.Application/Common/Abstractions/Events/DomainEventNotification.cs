using MediatR;
using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Application.Common.Abstractions.Events;

public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent)
    : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}