namespace Skill_Loop.UnitTests.Features.Sessions.Queries.GetSessionById;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionById;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetSessionByIdQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetSessionByIdQueryHandler _handler;

    public GetSessionByIdQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetSessionByIdQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var query = new GetSessionByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Session.NotFound");
    }

    [Fact]
    public async Task Handle_WhenSessionExists_ReturnsSuccessWithSessionResponse()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // استخدام الـ Result وطريقة الإنشاء بالبارامترات الجديدة
        var sessionResult = Session.Create(instructorId, ownerId, "Mastering C# and .NET", 100, 60, SessionType.Online);
        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSessionByIdQuery(session.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(session.Id);
        result.Data.Title.Should().Be("Mastering C# and .NET");
        result.Data.InstructorId.Should().Be(instructorId);
        result.Data.OwnerId.Should().Be(ownerId);
        result.Data.Status.Should().Be(SessionStatus.Draft);
    }
}