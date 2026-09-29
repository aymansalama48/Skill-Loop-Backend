namespace Skill_Loop.UnitTests.Features.Sessions.Commands.UpdateSession;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class UpdateSessionCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly UpdateSessionCommandHandler _handler;

    public UpdateSessionCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new UpdateSessionCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new UpdateSessionCommand(Guid.NewGuid(), "Updated Title");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Session.NotFound");
    }

    [Fact]
    public async Task Handle_WhenSessionExists_UpdatesTitleAndReturnsSuccess()
    {
        // Arrange
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Original Title");
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateSessionCommand(session.Id, "Updated New Title");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedSession = await _dbContext.FirstOrDefaultAsync(_dbContext.Sessions.Where(s => s.Id == session.Id));
        updatedSession.Should().NotBeNull();
        updatedSession!.Title.Should().Be("Updated New Title");
    }
}
