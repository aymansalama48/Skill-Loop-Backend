using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking.Events;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Booking;

/// <summary>
/// Ø­Ø¬Ø² Ù…ØªØ¹Ù„Ù… Ù„Ø¬Ù„Ø³Ø© Ù„Ø§ÙŠÙ (Live Session).
/// Ø§Ù„Ù€ Credits Ø¨ØªØªØ®ØµÙ… ÙˆÙ‚Øª Ø§Ù„Ø­Ø¬Ø² ÙˆØ¨ØªØ±Ø¬Ø¹ Ù„Ù„Ù…ØªØ¹Ù„Ù… Ù„Ùˆ Ø§ØªÙ„ØºÙ‰ØŒ ÙˆØ¨ØªØ±ÙˆØ­ Ù„Ù„Ù…Ø­Ø§Ø¶Ø± Ù„Ù…Ø§ Ø§Ù„Ø¬Ù„Ø³Ø© ØªØ®Ù„Øµ.
/// </summary>
public class Booking : AuditableEntity
{
    public Guid SessionId { get; private set; }
    public Guid LearnerUserId { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Confirmed;

    /// <summary>Ø§Ù„Ø³Ø¹Ø± ÙˆÙ‚Øª Ø§Ù„Ø­Ø¬Ø² (snapshot) Ø¹Ø´Ø§Ù† Ø§Ù„Ø³Ø¹Ø± ÙŠØªØºÙŠØ± Ø¨Ø¹Ø¯ÙŠÙ†</summary>
    public int PriceInCredits { get; private set; }

    /// <summary>Ù†Ø³Ø®Ø© Ù…Ù† Ù…ÙˆØ¹Ø¯ Ø§Ù„Ø¬Ù„Ø³Ø© ÙˆÙ‚Øª Ø§Ù„Ø­Ø¬Ø² (snapshot)</summary>
    public DateTime? ScheduledAtUtc { get; private set; }

    public DateTime BookedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    private Booking() { } // EF Core

    public static Result<Booking> Create(
        Guid sessionId,
        Guid learnerUserId,
        int priceInCredits,
        DateTime? scheduledAtUtc = null,
        DateTime? bookedAtUtc = null)
    {
        if (sessionId == Guid.Empty)
            return Result<Booking>.Failure(BookingDomainErrors.SessionIdRequired);

        if (learnerUserId == Guid.Empty)
            return Result<Booking>.Failure(BookingDomainErrors.LearnerIdRequired);

        if (priceInCredits < 0)
            return Result<Booking>.Failure(BookingDomainErrors.InvalidPrice);

        var booking = new Booking
        {
            Id = Guid.CreateVersion7(),
            SessionId = sessionId,
            LearnerUserId = learnerUserId,
            PriceInCredits = priceInCredits,
            ScheduledAtUtc = scheduledAtUtc,
            BookedAtUtc = bookedAtUtc ?? DateTime.UtcNow,
            Status = BookingStatus.Confirmed
        };

        booking.AddDomainEvent(new BookingCreatedDomainEvent(
            booking.Id,
            booking.SessionId,
            learnerUserId,
            booking.PriceInCredits,
            booking.ScheduledAtUtc));

        return Result<Booking>.Success(booking);
    }

    /// <summary>
    /// Ø§Ù„Ø­Ø§Ù„Ø§Øª Ø§Ù„Ù„ÙŠ Ø¨ØªØ¹ØªØ¨Ø± Ø§Ù„Ø­Ø¬Ø² "Ø´ØºØ§Ù„" (Ø¨ØªÙ„ØºÙŠ Ø§Ù„Ù…Ù‚Ø¹Ø¯ Ù…Ù† Ø§Ù„Ø¬Ù„Ø³Ø©)
    /// </summary>
    public static bool IsActiveStatus(BookingStatus status) =>
        status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.InProgress or BookingStatus.Completed;

    public bool IsActive => IsActiveStatus(Status);

    public bool CanBeCancelled => Status is BookingStatus.Pending or BookingStatus.Confirmed;

    /// <summary>
    /// Ù‡Ù„ Ø§Ù„Ø­Ø¬Ø² Ø¯Ù‡ Ù‚Ø§Ø¨Ù„ Ù„Ù„Ø§Ø³ØªØ±Ø¬Ø§Ø¹ØŸ (Ù„Ù…Ø§ ÙŠÙƒÙˆÙ† Ø§ØªØ¹Ù…Ù„ Ø£Ùˆ Ø§ØªØ±ÙØ¶ Ù‚Ø¨Ù„ Ù…Ø§ ØªØ¨Ø¯Ø£ Ø§Ù„Ø¬Ù„Ø³Ø©)
    /// </summary>
    public bool IsRefundable => Status is BookingStatus.Pending or BookingStatus.Confirmed;

    public Result Confirm()
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(BookingDomainErrors.InvalidTransition(Status, BookingStatus.Confirmed));

        Status = BookingStatus.Confirmed;
        return Result.Success();
    }

    public Result Start(DateTime? startedAtUtc = null)
    {
        if (Status != BookingStatus.Confirmed)
            return Result.Failure(BookingDomainErrors.InvalidTransition(Status, BookingStatus.InProgress));

        Status = BookingStatus.InProgress;
        StartedAtUtc = startedAtUtc ?? DateTime.UtcNow;
        return Result.Success();
    }

    public Result Reject(string? reason = null, DateTime? cancelledAtUtc = null)
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            return Result.Failure(BookingDomainErrors.InvalidTransition(Status, BookingStatus.Rejected));

        var wasRefundable = IsRefundable;
        Status = BookingStatus.Rejected;
        CancelledAtUtc = cancelledAtUtc ?? DateTime.UtcNow;
        CancellationReason = reason;

        AddDomainEvent(new BookingCancelledDomainEvent(
            Id, SessionId, LearnerUserId, PriceInCredits, wasRefundable, reason ?? string.Empty));

        return Result.Success();
    }

    public Result Cancel(string? reason = null, DateTime? cancelledAtUtc = null)
    {
        if (!CanBeCancelled)
            return Result.Failure(BookingDomainErrors.InvalidTransition(Status, BookingStatus.Cancelled));

        var wasRefundable = IsRefundable;
        Status = BookingStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc ?? DateTime.UtcNow;
        CancellationReason = reason;

        AddDomainEvent(new BookingCancelledDomainEvent(
            Id, SessionId, LearnerUserId, PriceInCredits, wasRefundable, reason ?? string.Empty));

        return Result.Success();
    }

    public Result MarkNoShow(DateTime? cancelledAtUtc = null)
    {
        if (Status is not (BookingStatus.Confirmed or BookingStatus.InProgress))
            return Result.Failure(BookingDomainErrors.InvalidTransition(Status, BookingStatus.NoShow));

        Status = BookingStatus.NoShow;
        CancelledAtUtc = cancelledAtUtc ?? DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Ø¥Ù†Ù‡Ø§Ø¡ Ø§Ù„Ø­Ø¬Ø²: Ø¨ÙŠØ±ÙØ¹ SessionCompletedDomainEvent Ø§Ù„Ù„ÙŠ Ø¨ÙŠØ®Ù„ÙŠ
    /// (Ø£) Ø¹Ø¯Ø§Ø¯ Ø¬Ù„Ø³Ø§Øª Ø§Ù„Ù…Ø­Ø§Ø¶Ø± ÙŠØ²ÙŠØ¯ Ùˆ (Ø¨) Ø±ØµÙŠØ¯ Ø§Ù„Ù…Ø­Ø§Ø¶Ø± ÙŠØ²ÙŠØ¯ ÙÙŠ Ø§Ù„Ù…Ø­ÙØ¸Ø©.
    /// </summary>
    public Result Complete(Guid instructorId, DateTime? completedAtUtc = null)
    {
        if (Status is not (BookingStatus.Confirmed or BookingStatus.InProgress))
            return Result.Failure(BookingDomainErrors.InvalidTransition(Status, BookingStatus.Completed));

        Status = BookingStatus.Completed;
        CompletedAtUtc = completedAtUtc ?? DateTime.UtcNow;
        StartedAtUtc ??= CompletedAtUtc;

        AddDomainEvent(new Sessions.Events.SessionCompletedDomainEvent(
            SessionId, instructorId, LearnerUserId, PriceInCredits));

        return Result.Success();
    }
}
