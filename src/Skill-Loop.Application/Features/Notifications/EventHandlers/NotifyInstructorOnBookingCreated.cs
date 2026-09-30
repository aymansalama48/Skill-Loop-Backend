using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking.Events;
using Skill_Loop.Domain.Entities.Notifications;

namespace Skill_Loop.Application.Features.Notifications.EventHandlers;

public sealed class NotifyInstructorOnBookingCreated : INotificationHandler<DomainEventNotification<BookingCreatedDomainEvent>>
{
    private readonly IApplicationDbContext _context;

    public NotifyInstructorOnBookingCreated(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DomainEventNotification<BookingCreatedDomainEvent> notificationWrapper, CancellationToken cancellationToken)
    {
        var notification = notificationWrapper.DomainEvent;
        var session = await _context.FirstOrDefaultAsync(
            _context.Sessions.Where(s => s.Id == notification.SessionId),
            cancellationToken);

        if (session is null) return;

        var notifResult = Notification.Create(
            session.InstructorId,
            "BookingCreated",
            "New Booking!",
            $"A new learner has booked your session '{session.Title}'.",
            $"{{\"SessionId\": \"{session.Id}\", \"BookingId\": \"{notification.BookingId}\"}}");

        if (notifResult.IsSuccess && notifResult.Data != null)
        {
            _context.Add(notifResult.Data);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
