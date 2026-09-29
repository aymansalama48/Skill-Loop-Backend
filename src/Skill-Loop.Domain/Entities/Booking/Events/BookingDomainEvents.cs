using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Entities.Booking.Events;

// ==========================================================
// أحداث الـ Booking — بتتشغّل عن طريق الـ Outbox + MediatR
// ==========================================================

/// <summary>
/// اتعمل حجز جديد — المبلغ اتخصم من محفظة المتعلم.
/// </summary>
public sealed record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid SessionId,
    Guid LearnerUserId,
    int PriceInCredits,
    DateTime? ScheduledAtUtc) : IDomainEvent;

/// <summary>
/// الحجز اتلغى أو اترفض — لو Refundable = true المفروض الـ credits ترجع للمتعلم.
/// </summary>
public sealed record BookingCancelledDomainEvent(
    Guid BookingId,
    Guid SessionId,
    Guid LearnerUserId,
    int RefundAmountInCredits,
    bool Refundable,
    string Reason) : IDomainEvent;
