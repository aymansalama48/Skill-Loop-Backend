namespace Skill_Loop.Domain.Common.Entities;

/// <summary>
/// كيان مزود بمفتاح أساسي من نوع Guid مع إصدار V7 (مرتب زمنياً)
/// </summary>
public abstract class BaseEntity : Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}