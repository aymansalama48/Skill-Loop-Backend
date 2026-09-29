using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Support.Commands.SendFaqAnswerEmail;

public class SendFaqAnswerEmailCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ISupportAnswerNotifier> _notificationService;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly SendFaqAnswerEmailCommandHandler _handler;

    public SendFaqAnswerEmailCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _notificationService = new Mock<ISupportAnswerNotifier>();
        _currentUser = new Mock<ICurrentUser>();
        _handler = new SendFaqAnswerEmailCommandHandler(
            _dbContext,
            _notificationService.Object,
            _currentUser.Object,
            Mock.Of<ILogger<SendFaqAnswerEmailCommandHandler>>());

        _currentUser.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.Email).Returns("reader@test.com");
        _currentUser.Setup(c => c.FullName).Returns("Reader");

        _notificationService
            .Setup(n => n.SendFaqAnswerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    private async Task<SupportQuestion> SeedAsync(bool isPublished, bool isAnswered)
    {
        var entity = SupportQuestion.Create("How do I reset?", "Account").Data!;
        if (isAnswered) entity.SetAnswer("Use the reset link.");
        if (isPublished) entity.Publish();

        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        _currentUser.Setup(c => c.IsAuthenticated).Returns(false);
        var question = await SeedAsync(isPublished: true, isAnswered: true);

        var result = await _handler.Handle(new SendFaqAnswerEmailCommand(question.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.Unauthenticated" && e.Type == ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_SendsTheAnswerToTheCurrentUserEmail()
    {
        var question = await SeedAsync(isPublished: true, isAnswered: true);

        var result = await _handler.Handle(new SendFaqAnswerEmailCommand(question.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _notificationService.Verify(n => n.SendFaqAnswerAsync(
            "reader@test.com",
            "Reader",
            "How do I reset?",
            "Use the reset link.",
            question.Id.ToString(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenQuestionIsNotPublished_ReturnsNotFound()
    {
        var draft = await SeedAsync(isPublished: false, isAnswered: true);

        var result = await _handler.Handle(new SendFaqAnswerEmailCommand(draft.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.AnswerNotAvailable" && e.Type == ErrorType.NotFound);
        _notificationService.Verify(n => n.SendFaqAnswerAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenQuestionIsUnanswered_ReturnsNotFound()
    {
        var pending = await SeedAsync(isPublished: false, isAnswered: false);

        var result = await _handler.Handle(new SendFaqAnswerEmailCommand(pending.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.AnswerNotAvailable");
    }

    [Fact]
    public async Task Handle_WhenEmailFails_ReturnsFailure()
    {
        _notificationService
            .Setup(n => n.SendFaqAnswerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("SupportNotification.EmailFailed", "smtp down", ErrorType.Failure)));

        var question = await SeedAsync(isPublished: true, isAnswered: true);

        var result = await _handler.Handle(new SendFaqAnswerEmailCommand(question.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportNotification.EmailFailed");
    }
}
