namespace Skill_Loop.Application.Features.Sessions.Materials.EventHandlers.SessionMaterialUploaded;

using MediatR;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.SessionsTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Entities.Session.Events;

public sealed class SessionMaterialUploadedEventHandler(
    IJobScheduler jobScheduler,
    IApplicationDbContext dbContext,
    IDateTime dateTime,
    IUserManagementService userService)
    : INotificationHandler<DomainEventNotification<SessionMaterialUploadedEvent>>
{
    public async Task Handle(
        DomainEventNotification<SessionMaterialUploadedEvent> notification,
        CancellationToken cancellationToken)
    {
        var material = notification.DomainEvent.Material;

        var session = await dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == material.SessionId, cancellationToken);

        if (session is null)
        {
            return;
        }

        var activeLearnerIds = await dbContext.Bookings
                    .AsNoTracking()
                    .Where(b =>
                        b.SessionId == material.SessionId &&
                        (b.Status == BookingStatus.Confirmed ||
                         b.Status == BookingStatus.Completed)) // تم حذف InProgress
                    .Select(b => b.LearnerUserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

        if (activeLearnerIds.Count == 0)
        {
            return;
        }

        var templateModel = new SessionMaterialUploadedTemplateModel
        {
            SessionTitle = session.Title,
            MaterialName = material.FileName,
            MaterialType = material.MaterialType?.ToString() ?? "Document",
            UploadedAt = dateTime.GetDateTimeString(),
            DownloadUrl = $"https://app.skill-loop.com/sessions/{material.SessionId}/materials/{material.Id}/download",
            InstructorName = string.Empty
        };

        var learners = await userService.GetUsersByIdsAsync(activeLearnerIds, cancellationToken);
        foreach (var learner in learners)
        {
            if (!string.IsNullOrEmpty(learner.Email))
            {
                jobScheduler.Enqueue<IIdentityNotificationService>(n =>
                    n.SendMaterialUploadedEmailAsync(learner.Email, templateModel));
            }
        }

        var instructor = await userService.GetByIdAsync(session.InstructorId, cancellationToken);
        if (instructor.IsSuccess)
        {
            var instructorEmail = instructor.Data?.Email;
            if (!string.IsNullOrEmpty(instructorEmail))
            {
                jobScheduler.Enqueue<IIdentityNotificationService>(n =>
                    n.SendMaterialUploadedConfirmationAsync(instructorEmail, templateModel));
            }
        }
    }
}
