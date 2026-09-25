using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Sessions.Events; // مسار الحدث اللي عملناه
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Sessions;

public sealed class Session : AuditableEntity
{
    // خليناهم private set للحماية
    public Guid InstructorId { get; private set; }
    public Guid OwnerId { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Draft;
    public string Title { get; private set; } = string.Empty;

    // استخدام Backing Field زي ما عملنا في Course و InstructorProfile
    private readonly List<SessionMaterial> _materials = new();
    public IReadOnlyCollection<SessionMaterial> Materials => _materials.AsReadOnly();

    private Session() { } // مطلوب لـ EF Core

    public static Session Create(Guid instructorId, Guid ownerId, string title)
    {
        return new Session
        {
            Id = Guid.CreateVersion7(), // عشان الـ Id يتولد صح
            InstructorId = instructorId,
            OwnerId = ownerId,
            Title = title.Trim(),
            Status = SessionStatus.Draft
        };
    }

    public void UpdateDetails(string title)
    {
        Title = title.Trim();
    }

    public void Publish()
    {
        if (Status != SessionStatus.Published)
        {
            Status = SessionStatus.Published;
        }
    }

    // ==========================================
    // هنا مربط الفرس: دالة إنهاء الجلسة اللي بترفع الحدث
    // ==========================================
    public Result Complete(Guid learnerUserId, int priceInCredits)
    {
        // (تقدر تضيف حالة Completed للـ SessionStatus لاحقاً وتغيرها هنا)

        // دي السطر اللي هيخلي الـ Outbox يلقط الحدث أوتوماتيك ويشغل الهاندلر!
        AddDomainEvent(new SessionCompletedDomainEvent(
            Id,
            InstructorId,
            learnerUserId,
            priceInCredits));

        return Result.Success();
    }
    /// <summary>
    /// تغيير حالة الجلسة
    /// </summary>
    public void ChangeStatus(SessionStatus newStatus)
    {
        Status = newStatus;
    }
    public void AddMaterial(SessionMaterial material)
    {
        _materials.Add(material);
    }

    public void RemoveMaterial(SessionMaterial material)
    {
        _materials.Remove(material);
    }
}