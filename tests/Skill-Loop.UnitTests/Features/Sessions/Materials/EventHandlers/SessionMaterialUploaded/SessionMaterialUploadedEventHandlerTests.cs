namespace Skill_Loop.UnitTests.Features.Sessions.Materials.EventHandlers.SessionMaterialUploaded;

using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.SessionsTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Materials.EventHandlers.SessionMaterialUploaded;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Session.Events;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class SessionMaterialUploadedEventHandlerTests
{
    private readonly Mock<IJobScheduler> _jobScheduler;
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IDateTime> _dateTime;
    private readonly Mock<IUserManagementService> _userService;
    private readonly SessionMaterialUploadedEventHandler _handler;

    public SessionMaterialUploadedEventHandlerTests()
    {
        _jobScheduler = new Mock<IJobScheduler>();
        _dbContext = InMemoryDbContextHelper.Create();
        _dateTime = new Mock<IDateTime>();
        _userService = new Mock<IUserManagementService>();
        _handler = new SessionMaterialUploadedEventHandler(
            _jobScheduler.Object,
            _dbContext,
            _dateTime.Object,
            _userService.Object);
    }

    [Fact]
    public async Task Handle_SessionNotFound_ReturnsEarly()
    {
        var sessionId = Guid.NewGuid();
        var material = SessionMaterial.Create(
            sessionId, "test.txt", "text/plain", 100, "file-1", null, 0,
            Guid.NewGuid(), MaterialType.Document);

        var notification = new DomainEventNotification<SessionMaterialUploadedEvent>(
            new SessionMaterialUploadedEvent(material));

        await _handler.Handle(notification, CancellationToken.None);

        _jobScheduler.Verify(j => j.Enqueue<IIdentityNotificationService>(It.IsAny<System.Linq.Expressions.Expression<System.Func<IIdentityNotificationService, System.Threading.Tasks.Task>>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoActiveLearners_ReturnsEarly()
    {
        var sessionId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();

        // استخدام الـ Result وطريقة الإنشاء الجديدة
        var sessionResult = Session.Create(instructorId, instructorId, "Test Session", 50, 60, SessionType.Online);
        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;
        session.Id = sessionId;

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var material = SessionMaterial.Create(
            sessionId, "test.txt", "text/plain", 100, "file-1", null, 0,
            Guid.NewGuid(), MaterialType.Document);

        var notification = new DomainEventNotification<SessionMaterialUploadedEvent>(
            new SessionMaterialUploadedEvent(material));

        await _handler.Handle(notification, CancellationToken.None);

        _jobScheduler.Verify(j => j.Enqueue<IIdentityNotificationService>(It.IsAny<System.Linq.Expressions.Expression<System.Func<IIdentityNotificationService, System.Threading.Tasks.Task>>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveLearnersAndInstructor_SendsNotifications()
    {
        var sessionId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();

        // استخدام الـ Result وطريقة الإنشاء الجديدة
        var sessionResult = Session.Create(instructorId, instructorId, "Test Session", 50, 60, SessionType.Online);
        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;
        session.Id = sessionId;

        // استخدام دالة الـ Create للـ Booking لتجنب أي مشاكل في الخصائص
        var bookingResult = Booking.Create(
            sessionId,
            learnerId,
            instructorId,
            DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(10),
            60,
            50,
            SessionType.Online);

        bookingResult.IsSuccess.Should().BeTrue();
        var booking = bookingResult.Data;

        _dbContext.Add(session);
        _dbContext.Add(booking);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var learner = new UserDto
        {
            Id = learnerId,
            Email = "learner@test.com",
        };

        _userService.Setup(s => s.GetUsersByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto> { learner });
        _userService.Setup(s => s.GetByIdAsync(instructorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(learner));

        _dateTime.Setup(d => d.GetDateTimeString()).Returns("2026-01-01T00:00:00Z");

        var material = SessionMaterial.Create(
            sessionId, "test.txt", "text/plain", 100, "file-1", null, 0,
            Guid.NewGuid(), MaterialType.Document);

        var notification = new DomainEventNotification<SessionMaterialUploadedEvent>(
            new SessionMaterialUploadedEvent(material));

        await _handler.Handle(notification, CancellationToken.None);

        _jobScheduler.Verify(j => j.Enqueue<IIdentityNotificationService>(It.IsAny<System.Linq.Expressions.Expression<System.Func<IIdentityNotificationService, System.Threading.Tasks.Task>>>()), Times.AtLeastOnce);
    }
}