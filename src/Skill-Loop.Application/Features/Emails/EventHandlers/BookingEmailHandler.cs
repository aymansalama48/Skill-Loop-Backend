using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking.Events;
using Skill_Loop.Domain.Entities.Emails;
using Skill_Loop.Domain.Common.Events;

using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;

namespace Skill_Loop.Application.Features.Emails.EventHandlers;

public class BookingEmailHandler : INotificationHandler<DomainEventNotification<BookingCreatedDomainEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly ILogger<BookingEmailHandler> _logger;
    private readonly IUserManagementService _userManagementService;

    public BookingEmailHandler(
        IApplicationDbContext context,
        IEmailTemplateEngine templateEngine,
        ILogger<BookingEmailHandler> logger,
        IUserManagementService userManagementService)
    {
        _context = context;
        _templateEngine = templateEngine;
        _logger = logger;
        _userManagementService = userManagementService;
    }

    public async Task Handle(DomainEventNotification<BookingCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == domainEvent.BookingId, cancellationToken);

        if (booking == null) return;

        var session = await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == domainEvent.SessionId, cancellationToken);
            
        if (session == null) return;
        
        var instructorProfile = await _context.InstructorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == session.InstructorId, cancellationToken);
            
        if (instructorProfile == null) return;

        var learnerUserResult = await _userManagementService.GetByIdAsync(domainEvent.LearnerUserId, cancellationToken);
        var instructorUserResult = await _userManagementService.GetByIdAsync(instructorProfile.UserId, cancellationToken);

        if (!learnerUserResult.IsSuccess || !instructorUserResult.IsSuccess) return;

        var learnerEmail = learnerUserResult.Data.Email;
        if (string.IsNullOrEmpty(learnerEmail)) return;

        var instructorName = instructorUserResult.Data.FullName;
        var learnerName = learnerUserResult.Data.FullName;

        var model = new
        {
            LearnerName = learnerName,
            InstructorName = instructorName,
            SessionDate = domainEvent.ScheduledAtUtc ?? session.ScheduledAtUtc,
            SessionTime = domainEvent.ScheduledAtUtc ?? session.ScheduledAtUtc,
            MeetingLink = session.LocationDetails ?? "سيتم إضافة الرابط قريباً",
            DashboardUrl = $"https://skillloop.com/dashboard/bookings/{booking.Id}",
            Year = DateTime.UtcNow.Year
        };

        var htmlBody = await _templateEngine.RenderTemplateAsync("BookingConfirmed", model);

        var emailLog = EmailLog.Create(
            type: "BookingConfirmed",
            recipientEmail: learnerEmail,
            referenceId: domainEvent.BookingId.ToString(), // Idempotency
            subject: "تأكيد الحجز - Skill Loop",
            body: htmlBody
        );

        _context.Add(emailLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Queued BookingConfirmed email for Learner {LearnerId}, Booking {BookingId}", domainEvent.LearnerUserId, domainEvent.BookingId);
    }
}

public class BookingCancelledEmailHandler : INotificationHandler<DomainEventNotification<BookingCancelledDomainEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly ILogger<BookingCancelledEmailHandler> _logger;
    private readonly IUserManagementService _userManagementService;

    public BookingCancelledEmailHandler(
        IApplicationDbContext context,
        IEmailTemplateEngine templateEngine,
        ILogger<BookingCancelledEmailHandler> logger,
        IUserManagementService userManagementService)
    {
        _context = context;
        _templateEngine = templateEngine;
        _logger = logger;
        _userManagementService = userManagementService;
    }

    public async Task Handle(DomainEventNotification<BookingCancelledDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        var learnerUserResult = await _userManagementService.GetByIdAsync(domainEvent.LearnerUserId, cancellationToken);
        if (!learnerUserResult.IsSuccess) return;

        var learnerEmail = learnerUserResult.Data.Email;
        if (string.IsNullOrEmpty(learnerEmail)) return;

        var model = new
        {
            LearnerName = learnerUserResult.Data.FullName,
            Reason = domainEvent.Reason ?? "لم يتم تحديد سبب",
            DashboardUrl = $"https://skillloop.com/dashboard/bookings/{domainEvent.BookingId}",
            Year = DateTime.UtcNow.Year
        };

        var htmlBody = await _templateEngine.RenderTemplateAsync("BookingCancelled", model);

        var emailLog = EmailLog.Create(
            type: "BookingCancelled",
            recipientEmail: learnerEmail,
            referenceId: $"{domainEvent.BookingId}-cancelled", // Idempotency
            subject: "إلغاء حجز - Skill Loop",
            body: htmlBody
        );

        _context.Add(emailLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Queued BookingCancelled email for Learner {LearnerId}, Booking {BookingId}", domainEvent.LearnerUserId, domainEvent.BookingId);
    }
}
