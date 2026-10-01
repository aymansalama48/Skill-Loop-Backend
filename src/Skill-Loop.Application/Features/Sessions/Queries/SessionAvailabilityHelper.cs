using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Queries;

/// <summary>
/// حساب المقاعد المتاحة للحجز (Active bookings) وبناء الـ DTO
/// </summary>
public static class SessionAvailabilityHelper
{
    public static Task<int> CountActiveBookingsAsync(
        IApplicationDbContext dbContext,
        Guid sessionId,
        CancellationToken cancellationToken) =>
        dbContext.Bookings
            .AsNoTracking()
            .CountAsync(b =>
                b.SessionId == sessionId &&
                (b.Status == BookingStatus.Pending ||
                 b.Status == BookingStatus.Confirmed ||
                 b.Status == BookingStatus.InProgress ||
                 b.Status == BookingStatus.Completed),
                cancellationToken);

    public static SessionResponse BuildResponse(
        Session session,
        int bookedParticipants,
        DateTime utcNow,
        string instructorName = "Unknown",
        string? instructorAvatarUrl = null) =>
        new(
            session.Id,
            session.InstructorId,
            session.OwnerId,
            instructorName,
            instructorAvatarUrl,
            session.AverageRating,
            session.TotalReviews,
            session.Title,
            session.Description,
            session.Status,
            session.ScheduledAtUtc,
            session.EndsAtUtc,
            session.DurationMinutes,
            session.CreditsPrice,
            session.LocationType,
            session.LocationDetails,
            session.MaxParticipants,
            bookedParticipants,
            session.AvailableSlots(bookedParticipants),
            session.IsBookable(utcNow, bookedParticipants),
            session.CreatedAt);
}
