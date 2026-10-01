using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;

namespace Skill_Loop.Application.Features.Bookings.Shared;

/// <summary>
/// مساعد مشترك لتحويل الـ Bookings لـ DTOs من غير N+1 queries
/// (نحمّل الـ Sessions مرة واحدة ونعمل dictionary).
/// </summary>
public static class BookingResponseFactory
{
    public static async Task<List<BookingResponse>> CreateListAsync(
        IApplicationDbContext dbContext,
        IUserManagementService userService,
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

        var instructorIds = sessions.Values.Select(s => s.InstructorId).Distinct().ToList();
        var instructors = await userService.GetUsersByIdsAsync(instructorIds, cancellationToken);
        var instructorAvatars = instructors.ToDictionary(u => u.Id, u => u.AvatarUrl);
        var instructorNames = instructors.ToDictionary(u => u.Id, u => u.FullName ?? "Unknown");

        return bookings
            .Where(b => sessions.ContainsKey(b.SessionId))
            .Select(b => 
            {
                var session = sessions[b.SessionId];
                var insName = instructorNames.GetValueOrDefault(session.InstructorId, "Unknown");
                var insAvatar = instructorAvatars.GetValueOrDefault(session.InstructorId);
                return Create(b, session, insName, insAvatar);
            })
            .ToList();
    }

    public static BookingResponse Create(Booking booking, Session session, string instructorName = "Unknown", string? instructorAvatarUrl = null) =>
        new(
            booking.Id,
            booking.SessionId,
            session.Title,
            session.InstructorId,
            instructorName,
            instructorAvatarUrl,
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
