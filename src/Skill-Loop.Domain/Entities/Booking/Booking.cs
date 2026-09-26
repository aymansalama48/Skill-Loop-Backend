using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Booking;

public sealed class Booking : AuditableEntity
{
    public Guid SessionId { get; private set; }
    public Guid LearnerUserId { get; private set; }
    public Guid InstructorId { get; private set; } // عشان نوصل للمدرب بسرعة

    public DateTime ScheduleDate { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public int DurationInMinutes { get; private set; }

    public int PricePaid { get; private set; }
    public SessionType Type { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Confirmed;
    public string? MeetingUrl { get; private set; }

    private Booking() { }

    public static Result<Booking> Create(
        Guid sessionId,
        Guid learnerUserId,
        Guid instructorId,
        DateTime scheduleDate,
        TimeSpan startTime,
        int durationInMinutes,
        int pricePaid,
        SessionType type)
    {
        if (learnerUserId == instructorId)
            return Result<Booking>.Failure(new Error("Booking.SelfBooking", "لا يمكنك حجز جلسة لنفسك.", ErrorType.Conflict));

        if (scheduleDate.Date < DateTime.UtcNow.Date)
            return Result<Booking>.Failure(new Error("Booking.InvalidDate", "لا يمكن الحجز في تاريخ ماضي.", ErrorType.Validation));

        return Result<Booking>.Success(new Booking
        {
            Id = Guid.CreateVersion7(),
            SessionId = sessionId,
            LearnerUserId = learnerUserId,
            InstructorId = instructorId,
            ScheduleDate = scheduleDate.Date,
            StartTime = startTime,
            DurationInMinutes = durationInMinutes,
            PricePaid = pricePaid,
            Type = type,
            Status = BookingStatus.Confirmed // بما إن الدفع بيتم فوراً
        });
    }

    public void SetMeetingUrl(string url)
    {
        MeetingUrl = url.Trim();
    }

    public void Complete()
    {
        if (Status == BookingStatus.Confirmed)
        {
            Status = BookingStatus.Completed;
        }
    }
    public Result Cancel()
    {
        if (Status == BookingStatus.Completed)
            return Result.Failure(new Error("Booking.AlreadyCompleted", "لا يمكن إلغاء جلسة مكتملة.", ErrorType.Conflict));

        if (Status == BookingStatus.Cancelled)
            return Result.Failure(new Error("Booking.AlreadyCancelled", "هذا الحجز ملغى بالفعل.", ErrorType.Conflict));

        Status = BookingStatus.Cancelled;

        return Result.Success();
    }
}