using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Sessions.Events;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Sessions;

public sealed class Session : AuditableEntity
{
    public Guid InstructorId { get; private set; }
    public Guid OwnerId { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Draft;
    public string Title { get; private set; } = string.Empty;

    // الخصائص الجديدة المطابقة لـ UI
    public int PriceInCredits { get; private set; }
    public int DurationInMinutes { get; private set; }
    public SessionType Type { get; private set; }

    private readonly List<SessionMaterial> _materials = new();
    public IReadOnlyCollection<SessionMaterial> Materials => _materials.AsReadOnly();

    private Session() { } // مطلوب لـ EF Core

    // تحديث دالة الإنشاء لتشمل البيانات الجديدة
    public static Result<Session> Create(
        Guid instructorId,
        Guid ownerId,
        string title,
        int priceInCredits,
        int durationInMinutes,
        SessionType type)
    {
        if (priceInCredits < 0)
            return Result<Session>.Failure(new Error("Session.InvalidPrice", "السعر يجب أن يكون 0 أو أكثر.", ErrorType.Validation));

        if (durationInMinutes <= 0)
            return Result<Session>.Failure(new Error("Session.InvalidDuration", "مدة الجلسة يجب أن تكون أكبر من صفر.", ErrorType.Validation));

        return Result<Session>.Success(new Session
        {
            Id = Guid.CreateVersion7(),
            InstructorId = instructorId,
            OwnerId = ownerId,
            Title = title.Trim(),
            PriceInCredits = priceInCredits,
            DurationInMinutes = durationInMinutes,
            Type = type,
            Status = SessionStatus.Draft
        });
    }

    // دالة لتعديل بيانات الجلسة
    public void UpdateDetails(string title, int priceInCredits, int durationInMinutes, SessionType type)
    {
        Title = title.Trim();
        PriceInCredits = priceInCredits;
        DurationInMinutes = durationInMinutes;
        Type = type;
    }



    public void ChangeStatus(SessionStatus newStatus)
    {
        Status = newStatus;
    }

    // دالة الإنهاء بتستخدم PriceInCredits الموجودة في الجلسة نفسها
    public Result Complete(Guid learnerUserId)
    {
        AddDomainEvent(new SessionCompletedDomainEvent(
            Id,
            InstructorId,
            learnerUserId,
            PriceInCredits));

        return Result.Success();
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