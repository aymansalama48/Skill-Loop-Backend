using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Support.Commands.AnswerSupportQuestion;

public class AnswerSupportQuestionCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ISupportAnswerNotifier> _notificationService;
    private readonly AnswerSupportQuestionCommandHandler _handler;

    public AnswerSupportQuestionCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _notificationService = new Mock<ISupportAnswerNotifier>();
        _handler = new AnswerSupportQuestionCommandHandler(
            _dbContext,
            _notificationService.Object,
            Mock.Of<ILogger<AnswerSupportQuestionCommandHandler>>());

        _notificationService
            .Setup(n => n.SendAnswerNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    [Fact]
    public async Task Handle_WhenQuestionDoesNotExist_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new AnswerSupportQuestionCommand(Guid.NewGuid(), "A"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.NotFound" && e.Type == ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithEmptyAnswer_ReturnsValidationFailure()
    {
        var question = SupportQuestion.Create("Q", "General", "user@test.com", "User").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(
            new AnswerSupportQuestionCommand(question.Id, "   "), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.EmptyAnswer");
    }

    [Fact]
    public async Task Handle_SetsAnswerAndMarksAnswered()
    {
        var question = SupportQuestion.Create("Q", "General", "user@test.com", "User").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(
            new AnswerSupportQuestionCommand(question.Id, "Here is the answer"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.SupportQuestions.Where(sq => sq.Id == question.Id));
        stored!.Answer.Should().Be("Here is the answer");
        stored.IsAnswered.Should().BeTrue();
        stored.AnsweredAt.Should().NotBeNull();
        stored.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithPublish_True_PublishesTheQuestion()
    {
        var question = SupportQuestion.Create("Q", "General", "user@test.com", "User").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        await _handler.Handle(
            new AnswerSupportQuestionCommand(question.Id, "A", Publish: true), CancellationToken.None);

        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.SupportQuestions.Where(sq => sq.Id == question.Id));
        stored!.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmailsTheAnswerAndMarksEmailSent()
    {
        var question = SupportQuestion.Create("Q", "General", "user@test.com", "User").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        await _handler.Handle(new AnswerSupportQuestionCommand(question.Id, "A"), CancellationToken.None);

        _notificationService.Verify(n => n.SendAnswerNotificationAsync(
            "user@test.com", "User", "Q", "A", "General", question.Id.ToString(), It.IsAny<CancellationToken>()), Times.Once);

        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.SupportQuestions.Where(sq => sq.Id == question.Id));
        stored!.EmailSent.Should().BeTrue();
        stored.EmailSentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenAnswerEmailFails_KeepsTheAnswerAndDoesNotMarkEmailSent()
    {
        _notificationService
            .Setup(n => n.SendAnswerNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("SupportNotification.EmailFailed", "smtp down", ErrorType.Failure)));

        var question = SupportQuestion.Create("Q", "General", "user@test.com", "User").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new AnswerSupportQuestionCommand(question.Id, "A"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.SupportQuestions.Where(sq => sq.Id == question.Id));
        stored!.Answer.Should().Be("A");
        stored.IsAnswered.Should().BeTrue();
        stored.EmailSent.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithoutAskerEmail_DoesNotAttemptToSendEmail()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new AnswerSupportQuestionCommand(question.Id, "A"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _notificationService.Verify(n => n.SendAnswerNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
