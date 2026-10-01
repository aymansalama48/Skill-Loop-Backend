using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Domain.Entities.System;
using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Infrastructure.Persistence.Interceptors;

/// <summary>
/// معترض يقوم بتسجيل التغييرات في النظام (الإضافة، التعديل، والحذف) 
/// تلقائيًا كـ Audit Logs.
/// </summary>
public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditLogInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var userId = _currentUser.UserId;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                // Prevent infinite loop if AuditLog itself is being saved
                if (entry.Entity is AuditLog) continue;

                var changes = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified || entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                    {
                        changes[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                var entityId = entry.Property("Id").CurrentValue?.ToString() ?? "Unknown";

                var log = AuditLog.Create(
                    userId,
                    entry.State.ToString(),
                    entry.Entity.GetType().Name,
                    entityId,
                    JsonSerializer.Serialize(changes)
                );
                
                auditLogs.Add(log);
            }
        }

        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
