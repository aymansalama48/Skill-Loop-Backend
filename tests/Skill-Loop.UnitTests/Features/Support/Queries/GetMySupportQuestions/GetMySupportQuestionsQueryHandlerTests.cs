using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Queries.GetMySupportQuestions;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Support.Queries.GetMySupportQuestions;

public class GetMySupportQuestionsQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetMySupportQuestionsQueryHandler _handler;

    public GetMySupportQuestionsQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetMySupportQuestionsQueryHandler(_dbContext);
    }

    private async Task<SupportQuestion> SeedAsync(Guid userId, string question)
    {
        var entity = SupportQuestion.Create(question, "General", "user@test.com", "Test User", userId).Data!;
        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    private Task<Result<PagedResult<MySupportQuestionResponse>>> QueryAsync(Guid userId) =>
        _handler.Handle(
            new GetMySupportQuestionsQuery(userId, new PaginationParameters { PageNumber = 1, PageSize = 10 }),
            CancellationToken.None);

    [Fact]
    public async Task Handle_ReturnsOnlyTheCallersQuestions()
    {
        var me = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();

        await SeedAsync(me, "My question");
        await SeedAsync(someoneElse, "Their question");

        var result = await QueryAsync(me);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
        result.Data.Items[0].Question.Should().Be("My question");
    }

    [Fact]
    public async Task Handle_ExposesTheAnswerOnceTheTeamReplied()
    {
        var me = Guid.NewGuid();
        var question = await SeedAsync(me, "My question");

        var pending = await QueryAsync(me);
        pending.Data!.Items[0].IsAnswered.Should().BeFalse();
        pending.Data.Items[0].Answer.Should().BeNull();

        question.SetAnswer("Here you go");
        _dbContext.Update(question);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var answered = await QueryAsync(me);
        answered.Data!.Items[0].IsAnswered.Should().BeTrue();
        answered.Data.Items[0].Answer.Should().Be("Here you go");
        answered.Data.Items[0].AnsweredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReturnsEmptyForUserWithoutQuestions()
    {
        var result = await QueryAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
    }
}
