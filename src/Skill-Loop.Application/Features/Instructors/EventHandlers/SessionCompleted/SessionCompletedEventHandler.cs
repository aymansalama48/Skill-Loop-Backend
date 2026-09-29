using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;

using Skill_Loop.Domain.Entities.Sessions.Events;

namespace Skill_Loop.Application.Features.Instructors.EventHandlers.SessionCompleted;

public sealed class SessionCompletedEventHandler(
    IApplicationDbContext _dbContext,
    ICacheService _cacheService) : INotificationHandler<DomainEventNotification<SessionCompletedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<SessionCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // 1. جلب بروفايل المدرب بناءً على InstructorId المبعوث في الحدث
        var profile = await _dbContext.InstructorProfiles
            .FirstOrDefaultAsync(p => p.UserId == domainEvent.InstructorId, cancellationToken);

        if (profile is null) return;

        // 2. زيادة عداد الجلسات
        profile.IncrementSessionsCompleted();

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 3. مسح الكاش عشان العداد الجديد يظهر
        var cacheKey = $"instructor-full-profile-userid-{profile.UserId}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
    }
}