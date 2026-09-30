using Microsoft.EntityFrameworkCore.Diagnostics;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Domain.Common.Events;
using Skill_Loop.Infrastructure.Persistence.Outbox;
using System.Text.Json;

namespace Skill_Loop.Infrastructure.Persistence.Interceptors;

public sealed class InsertOutboxMessagesInterceptor(
    IDateTime dateTime)
    : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
        {
            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        var occurredOnUtc = dateTime.UtcNow;

        var outboxMessages = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                return events;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = occurredOnUtc,
                Type = domainEvent.GetType().AssemblyQualifiedName
                    ?? domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(
                    domainEvent,
                    domainEvent.GetType()),
                RetryCount = 0
            })
            .ToList();

        if (outboxMessages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}