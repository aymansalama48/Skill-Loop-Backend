namespace Skill_Loop.UnitTests.Features.Sessions.Commands.CreateSession;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Commands.CreateSession;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CreateSessionCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly CreateSessionCommandHandler _handler;

    public CreateSessionCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new CreateSessionCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesSessionWithDraftStatusAndReturnsId()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var command = new CreateSessionCommand("Introduction to ASP.NET Core", instructorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        var createdSession = await _dbContext.FirstOrDefaultAsync(_dbContext.Sessions.Where(s => s.Id == result.Data));
        createdSession.Should().NotBeNull();
        createdSession!.Title.Should().Be("Introduction to ASP.NET Core");
        createdSession.InstructorId.Should().Be(instructorId);
        createdSession.OwnerId.Should().Be(instructorId);
        createdSession.Status.Should().Be(SessionStatus.Draft);
    }
}
