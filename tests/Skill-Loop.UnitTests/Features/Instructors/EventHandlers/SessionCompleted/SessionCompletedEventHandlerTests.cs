using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Instructors.EventHandlers.SessionCompleted;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.Domain.Entities.Sessions.Events;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Instructors.EventHandlers.SessionCompleted;

public class SessionCompletedEventHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ICacheService> _cacheService;
    private readonly SessionCompletedEventHandler _handler;

    public SessionCompletedEventHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _cacheService = new Mock<ICacheService>();
        _handler = new SessionCompletedEventHandler(_dbContext, _cacheService.Object);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsEarly()
    {
        var instructorId = Guid.NewGuid();

        var notification = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(Guid.NewGuid(), instructorId, Guid.NewGuid(), 10));

        await _handler.Handle(notification, CancellationToken.None);

        _cacheService.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidEvent_IncrementsSessionsCompletedAndInvalidatesCache()
    {
        var instructorId = Guid.NewGuid();
        var profile = InstructorProfile.Create(instructorId, "H", "B").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(Guid.NewGuid(), instructorId, Guid.NewGuid(), 10));

        await _handler.Handle(notification, CancellationToken.None);

        profile.SessionsCompleted.Should().Be(1);
        _cacheService.Verify(c => c.RemoveAsync($"instructor-full-profile-userid-{instructorId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CalledMultipleTimes_IncrementsMultipleTimes()
    {
        var instructorId = Guid.NewGuid();
        var profile = InstructorProfile.Create(instructorId, "H", "B").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification1 = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(Guid.NewGuid(), instructorId, Guid.NewGuid(), 10));
        var notification2 = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(Guid.NewGuid(), instructorId, Guid.NewGuid(), 10));

        await _handler.Handle(notification1, CancellationToken.None);
        await _handler.Handle(notification2, CancellationToken.None);

        profile.SessionsCompleted.Should().Be(2);
    }
}
