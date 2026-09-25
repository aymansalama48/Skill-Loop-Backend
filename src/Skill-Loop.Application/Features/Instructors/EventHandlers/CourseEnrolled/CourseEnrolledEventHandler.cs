using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Enrollments.Events;

namespace Skill_Loop.Application.Features.Instructors.EventHandlers.CourseEnrolled;

public sealed class CourseEnrolledEventHandler(
    IApplicationDbContext _dbContext,
    ICacheService _cacheService) : INotificationHandler<DomainEventNotification<CourseEnrolledDomainEvent>>
{
    public async Task Handle(DomainEventNotification<CourseEnrolledDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // تجاهل لو الكورس مجاني
        if (domainEvent.CreditsPaid <= 0) return;

        // 1. جلب الكورس لمعرفة معرّف المدرب
        var course = await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == domainEvent.CourseId, cancellationToken);

        if (course is null) return;

        // 2. جلب بروفايل المدرب
        var profile = await _dbContext.InstructorProfiles
            .FirstOrDefaultAsync(p => p.UserId == course.InstructorId, cancellationToken);

        if (profile is null) return;

        // 3. إضافة الكريديت للبروفايل
        profile.AddCreditsEarned(domainEvent.CreditsPaid);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 4. مسح الكاش الخاص بالبروفايل عشان الأرباح الجديدة تظهر فوراً
        var cacheKey = $"instructor-full-profile-userid-{profile.UserId}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
    }
}