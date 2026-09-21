namespace Skill_Loop.Domain.Common.Entities;


/// <summary>
/// كيان يدعم تتبع الإنشاء والتعديل (بدون حذف منطقي)
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}