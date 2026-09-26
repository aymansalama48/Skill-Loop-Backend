namespace Skill_Loop.UnitTests.Features.Sessions.Commands.UpdateSession;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
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
        // Arrange - تمرير البارامترات المطلوبة للأمر (Id, Title, Price, Duration, Type)
        var command = new UpdateSessionCommand(Guid.NewGuid(), "Updated Title", 100, 60, SessionType.Online);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Session.NotFound");
    }

    [Fact]
    public async Task Handle_WhenSessionExists_UpdatesTitleAndReturnsSuccess()
    {
        // Arrange - إنشاء الجلسة بالطريقة الجديدة
        var sessionResult = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Original Title", 50, 30, SessionType.Online);
        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // إرسال الأمر بالتحديث بالبيانات الجديدة
        var command = new UpdateSessionCommand(session.Id, "Updated New Title", 120, 45, SessionType.Offline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedSession = await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        updatedSession.Should().NotBeNull();
        updatedSession!.Title.Should().Be("Updated New Title");
        updatedSession.PriceInCredits.Should().Be(120);
        updatedSession.DurationInMinutes.Should().Be(45);
        updatedSession.Type.Should().Be(SessionType.Offline);
    }
}