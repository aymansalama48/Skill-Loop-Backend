using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Sessions;

public class Session : AuditableEntity
{
    public Guid InstructorId { get; set; }
    public Guid OwnerId { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Draft;
    public string Title { get; set; } = string.Empty;

    public ICollection<SessionMaterial> Materials { get; set; } = new List<SessionMaterial>();

    private Session() { } // مطلوب لـ EF Core

    public static Session Create(Guid instructorId, Guid ownerId, string title)
    {
        return new Session
        {
            InstructorId = instructorId,
            OwnerId = ownerId,
            Title = title,
            Status = SessionStatus.Draft
        };
    }

    // ==========================================
    // الميثودات المساعدة (Domain Logic Methods)
    // ==========================================

    /// <summary>
    /// تحديث بيانات الجلسة الأساسية
    /// </summary>
    public void UpdateDetails(string title)
    {
        Title = title;
    }

    /// <summary>
    /// نشر الجلسة لتصبح متاحة للطلاب
    /// </summary>
    public void Publish()
    {
        if (Status != SessionStatus.Published)
        {
            Status = SessionStatus.Published;
        }
    }

    /// <summary>
    /// إنهاء أو إلغاء الجلسة (بافتراض وجود حالة Cancelled أو Completed في الـ Enum)
    /// </summary>
    public void ChangeStatus(SessionStatus newStatus)
    {
        Status = newStatus;
    }

    /// <summary>
    /// مساعدة لإضافة ملف جديد للجلسة
    /// </summary>
    public void AddMaterial(SessionMaterial material)
    {
        Materials.Add(material);
    }

    /// <summary>
    /// مساعدة لحذف ملف من الجلسة
    /// </summary>
    public void RemoveMaterial(SessionMaterial material)
    {
        Materials.Remove(material);
    }
}