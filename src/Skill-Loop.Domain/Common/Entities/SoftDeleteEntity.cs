namespace Skill_Loop.Domain.Common.Entities;

/// <summary>
/// كيان يدعم الحذف المنطقي مع تتبع كامل للإنشاء والتعديل
/// </summary>
public abstract class SoftDeleteEntity : AuditableEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}