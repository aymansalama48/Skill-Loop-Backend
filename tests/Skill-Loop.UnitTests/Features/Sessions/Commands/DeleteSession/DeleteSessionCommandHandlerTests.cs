namespace Skill_Loop.UnitTests.Features.Sessions.Commands.DeleteSession;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Commands.DeleteSession;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class DeleteSessionCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly DeleteSessionCommandHandler _handler;

    public DeleteSessionCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new DeleteSessionCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new DeleteSessionCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Session.NotFound");
    }

    [Fact]
    public async Task Handle_WhenSessionHasMaterials_ReturnsConflictFailure()
    {
        // Arrange - استخدام الـ Result وطريقة الإنشاء الجديدة بالخصائص (السعر، المدة، النوع)
        var sessionResult = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session with files", 50, 60, SessionType.Online);
        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var material = SessionMaterial.Create(
            session.Id, "file.pdf", "application/pdf", 1024, "drive-1", null, 1, Guid.NewGuid());

        // استخدام الدومين لإضافة الملف للجلسة
        session.AddMaterial(material);
        _dbContext.Add(material);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteSessionCommand(session.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Session.HasMaterials");
    }

    [Fact]
    public async Task Handle_WhenSessionHasNoMaterials_RemovesSessionAndReturnsSuccess()
    {
        // Arrange - استخدام الـ Result وطريقة الإنشاء الجديدة
        var sessionResult = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session to delete", 50, 60, SessionType.Online);
        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteSessionCommand(session.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedSession = await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        deletedSession.Should().BeNull();
    }
}