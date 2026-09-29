using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Commands.SetSupportQuestionPublication;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionsPaged;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Support.Commands.SetSupportQuestionPublication;

public class SetSupportQuestionPublicationCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly SetSupportQuestionPublicationCommandHandler _handler;
    private readonly GetSupportQuestionsPagedQueryHandler _pagedHandler;

    public SetSupportQuestionPublicationCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new SetSupportQuestionPublicationCommandHandler(_dbContext);
        _pagedHandler = new GetSupportQuestionsPagedQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenQuestionDoesNotExist_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new SetSupportQuestionPublicationCommand(Guid.NewGuid(), true), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.NotFound" && e.Type == ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_PublishesAQuestionThatHasAnAnswer()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;
        question.SetAnswer("A");
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(
            new SetSupportQuestionPublicationCommand(question.Id, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.SupportQuestions.Where(sq => sq.Id == question.Id));
        stored!.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RefusesToPublishWithoutAnAnswer()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(
            new SetSupportQuestionPublicationCommand(question.Id, true), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.NoAnswer");
    }

    [Fact]
    public async Task Handle_UnpublishesAnAnsweredQuestion()
    {
        var question = SupportQuestion.CreatePublished("Q", "A", "General").Data!;
        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(
            new SetSupportQuestionPublicationCommand(question.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.SupportQuestions.Where(sq => sq.Id == question.Id));
        stored!.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task PagedQueue_FiltersUnansweredAndExposesAskerDetails()
    {
        var pending = SupportQuestion.Create("Pending question", "General", "user@test.com", "Test User").Data!;
        var answered = SupportQuestion.CreatePublished("Answered question", "A", "General").Data!;
        _dbContext.Add(pending);
        _dbContext.Add(answered);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _pagedHandler.Handle(
            new GetSupportQuestionsPagedQuery(
                new PaginationParameters { PageNumber = 1, PageSize = 10 },
                IsAnswered: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
        result.Data.Items[0].Id.Should().Be(pending.Id);
        result.Data.Items[0].UserEmail.Should().Be("user@test.com");
        result.Data.Items[0].IsPublished.Should().BeFalse();
    }
}
