using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.System;

/// <summary>
/// سجل تغييرات النظام
/// </summary>
public class AuditLog : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? Changes { get; private set; }
    public DateTime Timestamp { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid? userId,
        string action,
        string entityType,
        string entityId,
        string? changes)
    {
        return new AuditLog
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Changes = changes,
            Timestamp = DateTime.UtcNow
        };
    }
}
