namespace Skill_Loop.UnitTests.Features.Sessions.Commands.ChangeSessionStatus;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ChangeSessionStatusCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ChangeSessionStatusCommandHandler _handler;

    public ChangeSessionStatusCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new ChangeSessionStatusCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new ChangeSessionStatusCommand(Guid.NewGuid(), SessionStatus.Published);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Session.NotFound");
    }

    [Fact]
    public async Task Handle_WhenSessionExists_UpdatesStatusAndReturnsSuccess()
    {
        // Arrange - استخدام الـ Result والبارامترات الجديدة للإنشاء
        var sessionResult = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "My Test Session", 100, 60, SessionType.Online);
        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new ChangeSessionStatusCommand(session.Id, SessionStatus.Published);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedSession = await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        updatedSession.Should().NotBeNull();
        updatedSession!.Status.Should().Be(SessionStatus.Published);
    }
}