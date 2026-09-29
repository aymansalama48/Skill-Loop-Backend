using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Support.Commands.SubmitContactForm;

public class SubmitContactFormCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ISupportRequestNotifier> _notificationService;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly SubmitContactFormCommandHandler _handler;

    public SubmitContactFormCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _notificationService = new Mock<ISupportRequestNotifier>();
        _currentUser = new Mock<ICurrentUser>();
        _handler = new SubmitContactFormCommandHandler(
            _dbContext,
            _notificationService.Object,
            _currentUser.Object,
            Mock.Of<ILogger<SubmitContactFormCommandHandler>>());

        _notificationService
            .Setup(n => n.SendContactFormConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _notificationService
            .Setup(n => n.SendSupportTeamNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    private void Authenticate(Guid userId, string email = "user@test.com", string fullName = "Test User")
    {
        _currentUser.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _currentUser.Setup(c => c.Email).Returns(email);
        _currentUser.Setup(c => c.FullName).Returns(fullName);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        _currentUser.Setup(c => c.IsAuthenticated).Returns(false);

        var result = await _handler.Handle(new SubmitContactFormCommand("S", "M"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.Unauthenticated" && e.Type == ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedWithoutEmail_ReturnsUnauthorized()
    {
        _currentUser.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUser.Setup(c => c.Email).Returns((string?)null);

        var result = await _handler.Handle(new SubmitContactFormCommand("S", "M"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.Unauthenticated");
    }

    [Fact]
    public async Task Handle_StoresQuestionOwnedByCurrentUserAndUnanswered()
    {
        var userId = Guid.NewGuid();
        Authenticate(userId);

        var result = await _handler.Handle(new SubmitContactFormCommand("Reset", "How do I reset?", "Account"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.SupportQuestions.Where(sq => sq.Id == result.Data));
        stored.Should().NotBeNull();
        stored!.AskedByUserId.Should().Be(userId);
        stored.UserEmail.Should().Be("user@test.com");
        stored.UserName.Should().Be("Test User");
        stored.Category.Should().Be("Account");
        stored.Question.Should().Contain("Reset");
        stored.Question.Should().Contain("How do I reset?");
        stored.IsAnswered.Should().BeFalse();
        stored.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SendsConfirmationToUserAndNotificationToTeam()
    {
        Authenticate(Guid.NewGuid());

        await _handler.Handle(new SubmitContactFormCommand("Reset", "How do I reset?", "Account"), CancellationToken.None);

        _notificationService.Verify(n => n.SendContactFormConfirmationAsync(
            "user@test.com", "Test User", "Reset", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        _notificationService.Verify(n => n.SendSupportTeamNotificationAsync(
            "Test User", "user@test.com", "Reset", "How do I reset?", "Account", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailsFail_StillReturnsSuccess()
    {
        Authenticate(Guid.NewGuid());
        _notificationService
            .Setup(n => n.SendContactFormConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("SupportNotification.EmailFailed", "smtp down", ErrorType.Failure)));
        _notificationService
            .Setup(n => n.SendSupportTeamNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("SupportNotification.EmailFailed", "smtp down", ErrorType.Failure)));

        var result = await _handler.Handle(new SubmitContactFormCommand("S", "M"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFullNameMissing_FallsBackToEmail()
    {
        Authenticate(Guid.NewGuid(), "user@test.com", fullName: null!);

        await _handler.Handle(new SubmitContactFormCommand("S", "M"), CancellationToken.None);

        _notificationService.Verify(n => n.SendSupportTeamNotificationAsync(
            "user@test.com", "user@test.com", "S", "M", "General", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
