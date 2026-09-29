using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Enums;
using BookingEntity = Skill_Loop.Domain.Entities.Booking.Booking;

namespace Skill_Loop.Domain.Entities.Sessions;

public sealed class Session : AuditableEntity
{
    public const int DefaultDurationMinutes = 60;
    public const int MaxDurationMinutes = 480;
    public const int MaxParticipantsLimit = 50;

    // خليناهم private set للحماية
    public Guid InstructorId { get; private set; }
    public Guid OwnerId { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Draft;
    public string Title { get; private set; } = string.Empty;

    // ===== حقول المواعيد والحجز (Live Session Slot) =====
    public string? Description { get; private set; }
    public DateTime? ScheduledAtUtc { get; private set; }
    public int DurationMinutes { get; private set; } = DefaultDurationMinutes;
    public int CreditsPrice { get; private set; }
    public SessionLocationType LocationType { get; private set; } = SessionLocationType.Online;
    public string? LocationDetails { get; private set; }
    public int MaxParticipants { get; private set; } = 1;

    // استخدام Backing Field زي ما عملنا في Course و InstructorProfile
    private readonly List<SessionMaterial> _materials = new();
    public IReadOnlyCollection<SessionMaterial> Materials => _materials.AsReadOnly();

    private readonly List<BookingEntity> _bookings = new();
    public IReadOnlyCollection<BookingEntity> Bookings => _bookings.AsReadOnly();

    private Session() { } // مطلوب لـ EF Core

    public static Session Create(
        Guid instructorId,
        Guid ownerId,
        string title,
        string? description = null,
        DateTime? scheduledAtUtc = null,
        int durationMinutes = DefaultDurationMinutes,
        int creditsPrice = 0,
        SessionLocationType locationType = SessionLocationType.Online,
        string? locationDetails = null,
        int maxParticipants = 1)
    {
        return new Session
        {
            Id = Guid.CreateVersion7(), // عشان الـ Id يتولد صح
            InstructorId = instructorId,
            OwnerId = ownerId,
            Title = title?.Trim() ?? string.Empty,
            Description = Normalize(description),
            ScheduledAtUtc = scheduledAtUtc,
            DurationMinutes = ClampDuration(durationMinutes),
            CreditsPrice = Math.Max(0, creditsPrice),
            LocationType = locationType,
            LocationDetails = Normalize(locationDetails),
            MaxParticipants = ClampMaxParticipants(maxParticipants),
            Status = SessionStatus.Draft
        };
    }

    public void UpdateDetails(string title, string? description = null)
    {
        Title = title?.Trim() ?? string.Empty;

        // لو الـ description مبعتش (null) مش هنمسح الموجود، بنخليه زي ما هو
        if (description is not null)
        {
            Description = Normalize(description);
        }
    }

    /// <summary>
    /// تحديث بيانات الموعد (وقت / مدة / سعر / مكان / عدد المشاركين).
    /// أي معامل null أو 0 معناه "سيبه زي ما هو" (Partial Update).
    /// </summary>
    public void UpdateSchedule(
        DateTime? scheduledAtUtc = null,
        int? durationMinutes = null,
        int? creditsPrice = null,
        SessionLocationType? locationType = null,
        string? locationDetails = null,
        int? maxParticipants = null)
    {
        if (scheduledAtUtc.HasValue)
        {
            ScheduledAtUtc = scheduledAtUtc.Value;
        }

        if (durationMinutes.HasValue)
        {
            DurationMinutes = ClampDuration(durationMinutes.Value);
        }

        if (creditsPrice.HasValue)
        {
            CreditsPrice = Math.Max(0, creditsPrice.Value);
        }

        if (locationType.HasValue)
        {
            LocationType = locationType.Value;
        }

        if (locationDetails is not null)
        {
            LocationDetails = Normalize(locationDetails);
        }

        if (maxParticipants.HasValue)
        {
            MaxParticipants = ClampMaxParticipants(maxParticipants.Value);
        }
    }

    public void Publish()
    {
        if (Status != SessionStatus.Published)
        {
            Status = SessionStatus.Published;
        }
    }

    public void Cancel()
    {
        Status = SessionStatus.Cancelled;
    }

    /// <summary>
    /// هل الجلسة جاهزة للحجز؟ لازم تكون منشورة وموعلة وفي مقاعد فاضية
    /// </summary>
    public bool IsBookable(DateTime utcNow, int activeBookingsCount)
    {
        if (Status != SessionStatus.Published)
        {
            return false;
        }

        if (ScheduledAtUtc is null || ScheduledAtUtc.Value <= utcNow)
        {
            return false;
        }

        return activeBookingsCount < MaxParticipants;
    }

    public int AvailableSlots(int activeBookingsCount) =>
        Math.Max(0, MaxParticipants - activeBookingsCount);

    public DateTime? EndsAtUtc => ScheduledAtUtc?.AddMinutes(DurationMinutes);

    /// <summary>
    /// تغيير حالة الجلسة
    /// </summary>
    public void ChangeStatus(SessionStatus newStatus)
    {
        Status = newStatus;
    }

    // ==========================================
    // هنا مربط الفرس: دالة إنهاء الجلسة اللي بترفع الحدث
    // ==========================================
    public Result Complete()
    {
        Status = SessionStatus.Completed;
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

    public void AddBooking(BookingEntity booking)
    {
        _bookings.Add(booking);
    }

    public void RemoveBooking(BookingEntity booking)
    {
        _bookings.Remove(booking);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int ClampDuration(int minutes) =>
        minutes <= 0 ? DefaultDurationMinutes : Math.Min(minutes, MaxDurationMinutes);

    private static int ClampMaxParticipants(int participants) =>
        participants <= 0 ? 1 : Math.Min(participants, MaxParticipantsLimit);
}
