using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Queries.GetPublishedSupportQuestionsPaged;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Support.Queries.GetPublishedSupportQuestionsPaged;

public class GetPublishedSupportQuestionsPagedQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetPublishedSupportQuestionsPagedQueryHandler _handler;
    private readonly GetSupportQuestionByIdQueryHandler _byIdHandler;

    public GetPublishedSupportQuestionsPagedQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetPublishedSupportQuestionsPagedQueryHandler(_dbContext);
        _byIdHandler = new GetSupportQuestionByIdQueryHandler(_dbContext);
    }

    private async Task<SupportQuestion> SeedAsync(bool isPublished, bool isAnswered, string question = "How do I reset?")
    {
        var entity = SupportQuestion.Create(question, "Account", "user@test.com", "Test User").Data!;
        if (isAnswered)
            entity.SetAnswer("Use the reset link.");
        if (isPublished)
            entity.Publish();

        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    private Task<Result<PagedResult<SupportQuestionResponse>>> QueryAsync(string? searchTerm = null, string? category = null) =>
        _handler.Handle(
            new GetPublishedSupportQuestionsPagedQuery(
                new PaginationParameters { PageNumber = 1, PageSize = 10 },
                searchTerm,
                category),
            CancellationToken.None);

    [Fact]
    public async Task Handle_ExcludesUnpublishedQuestions()
    {
        await SeedAsync(isPublished: false, isAnswered: true, question: "Draft question");

        var result = await QueryAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_IncludesPublishedQuestions()
    {
        await SeedAsync(isPublished: true, isAnswered: true);

        var result = await QueryAsync();

        result.Data!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_SearchMatchesQuestionAndAnswer()
    {
        await SeedAsync(isPublished: true, isAnswered: true, question: "Refund timeline");

        var byQuestion = await QueryAsync("refund");
        var byAnswer = await QueryAsync("reset link");

        byQuestion.Data!.Items.Should().ContainSingle();
        byAnswer.Data!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_FiltersByCategoryCaseInsensitively()
    {
        await SeedAsync(isPublished: true, isAnswered: true);

        var result = await QueryAsync(category: "account");

        result.Data!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ReportsPaginationMetadata()
    {
        await SeedAsync(isPublished: true, isAnswered: true);
        await SeedAsync(isPublished: true, isAnswered: true, question: "Another one");

        var result = await QueryAsync();

        result.Data!.Pagination.TotalCount.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForUnpublishedQuestion()
    {
        var draft = await SeedAsync(isPublished: false, isAnswered: true);

        var result = await _byIdHandler.Handle(new GetSupportQuestionByIdQuery(draft.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.NotFound" && e.Type == ErrorType.NotFound);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForUnknownId()
    {
        var result = await _byIdHandler.Handle(new GetSupportQuestionByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.NotFound");
    }

    [Fact]
    public async Task GetById_ReturnsPublishedQuestion()
    {
        var published = await SeedAsync(isPublished: true, isAnswered: true);

        var result = await _byIdHandler.Handle(new GetSupportQuestionByIdQuery(published.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Answer.Should().Be("Use the reset link.");
    }
}
