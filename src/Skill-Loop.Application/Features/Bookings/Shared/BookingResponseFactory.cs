using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;

namespace Skill_Loop.Application.Features.Bookings.Shared;

/// <summary>
/// مساعد مشترك لتحويل الـ Bookings لـ DTOs من غير N+1 queries
/// (نحمّل الـ Sessions مرة واحدة ونعمل dictionary).
/// </summary>
public static class BookingResponseFactory
{
    public static async Task<List<BookingResponse>> CreateListAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<Booking> bookings,
        CancellationToken cancellationToken)
    {
        if (bookings.Count == 0)
        {
            return [];
        }

        var sessionIds = bookings.Select(b => b.SessionId).Distinct().ToList();

        var sessions = await dbContext.Sessions
            .AsNoTracking()
            .Where(s => sessionIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return bookings
            .Where(b => sessions.ContainsKey(b.SessionId))
            .Select(b => Create(b, sessions[b.SessionId]))
            .ToList();
    }

    public static BookingResponse Create(Booking booking, Session session) =>
        new(
            booking.Id,
            booking.SessionId,
            session.Title,
            session.InstructorId,
            booking.LearnerUserId,
            booking.PriceInCredits,
            booking.Status,
            booking.ScheduledAtUtc,
            session.DurationMinutes,
            session.LocationType,
            session.LocationDetails,
            booking.BookedAtUtc,
            booking.StartedAtUtc,
            booking.CompletedAtUtc,
            booking.CancelledAtUtc,
            booking.CancellationReason);
}
