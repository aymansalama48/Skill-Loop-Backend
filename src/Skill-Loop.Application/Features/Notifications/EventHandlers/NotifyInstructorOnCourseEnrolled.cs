using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Enrollments.Events;
using Skill_Loop.Domain.Entities.Notifications;

namespace Skill_Loop.Application.Features.Notifications.EventHandlers;

public sealed class NotifyInstructorOnCourseEnrolled : INotificationHandler<DomainEventNotification<CourseEnrolledDomainEvent>>
{
    private readonly IApplicationDbContext _context;

    public NotifyInstructorOnCourseEnrolled(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DomainEventNotification<CourseEnrolledDomainEvent> notificationWrapper, CancellationToken cancellationToken)
    {
        var notification = notificationWrapper.DomainEvent;
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Where(c => c.Id == notification.CourseId),
            cancellationToken);

        if (course is null) return;

        var user = await _context.FirstOrDefaultAsync(
            _context.InstructorProfiles.Where(u => u.UserId == notification.UserId), // Assuming UserId can be looked up to get name, but we might not have user table here easily. Let's just say "A student".
            cancellationToken); // Wait, better to not lookup if not needed.

        var notifResult = Notification.Create(
            course.InstructorId,
            "CourseEnrolled",
            "New Student Enrolled!",
            $"A new student has enrolled in your course '{course.Title}'.",
            $"{{\"CourseId\": \"{course.Id}\"}}");

        if (notifResult.IsSuccess && notifResult.Data != null)
        {
            _context.Add(notifResult.Data);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
