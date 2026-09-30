using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking.Events;
using Skill_Loop.Domain.Entities.Chat;

namespace Skill_Loop.Application.Features.Chat.EventHandlers;

public class StartChatOnBookingCreatedHandler : INotificationHandler<DomainEventNotification<BookingCreatedDomainEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<StartChatOnBookingCreatedHandler> _logger;

    public StartChatOnBookingCreatedHandler(
        IApplicationDbContext context,
        ILogger<StartChatOnBookingCreatedHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<BookingCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var session = await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == domainEvent.SessionId, cancellationToken);

        if (session == null) return;

        var instructorProfile = await _context.InstructorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == session.InstructorId, cancellationToken);

        if (instructorProfile == null) return;

        var learnerId = domainEvent.LearnerUserId;
        var instructorId = instructorProfile.UserId;

        // 1. لا تفتح محادثة مع نفسك
        if (learnerId == instructorId) return;

        // 2. دوّر على محادثة موجودة (الترتيب ثابت زي ما في الـ Domain)
        var (one, two) = Conversation.NormalizePair(learnerId, instructorId);

        var conversationExists = await _context.Conversations
            .AnyAsync(c => c.ParticipantOneId == one && c.ParticipantTwoId == two, cancellationToken);

        // 3. مفيش؟ اعمل واحدة جديدة
        if (!conversationExists)
        {
            var createResult = Conversation.Create(learnerId, instructorId);
            if (createResult.IsFailure)
            {
                _logger.LogWarning("Failed to auto-create conversation on booking: {Errors}", createResult.Errors);
                return;
            }

            _context.Add(createResult.Data!);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-created chat conversation between {LearnerId} and {InstructorId} for Booking {BookingId}", learnerId, instructorId, domainEvent.BookingId);
        }
    }
}
