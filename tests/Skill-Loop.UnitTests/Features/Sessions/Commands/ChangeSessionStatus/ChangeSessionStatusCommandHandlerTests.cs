namespace Skill_Loop.UnitTests.Features.Sessions.Commands.ChangeSessionStatus;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
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
        // Arrange
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "My Test Session");
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new ChangeSessionStatusCommand(session.Id, SessionStatus.Published);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedSession = await _dbContext.FirstOrDefaultAsync(_dbContext.Sessions.Where(s => s.Id == session.Id));
        updatedSession.Should().NotBeNull();
        updatedSession!.Status.Should().Be(SessionStatus.Published);
    }
}
