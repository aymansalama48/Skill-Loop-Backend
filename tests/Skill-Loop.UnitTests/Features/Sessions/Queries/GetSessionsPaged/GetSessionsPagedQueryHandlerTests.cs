namespace Skill_Loop.UnitTests.Features.Sessions.Queries.GetSessionsPaged;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetSessionsPagedQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetSessionsPagedQueryHandler _handler;

    public GetSessionsPagedQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetSessionsPagedQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenNoSessionsExist_ReturnsEmptyPagedResult()
    {
        // Arrange
        var query = new GetSessionsPagedQuery(new PaginationParameters { PageNumber = 1, PageSize = 10 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.Pagination.TotalCount.Should().Be(0);
        result.Data.Pagination.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithoutInstructorFilter_ReturnsAllSessionsPaged()
    {
        // Arrange
        var instructor1 = Guid.NewGuid();
        var instructor2 = Guid.NewGuid();

        var session1 = Session.Create(instructor1, instructor1, "Session 1");
        var session2 = Session.Create(instructor2, instructor2, "Session 2");
        var session3 = Session.Create(instructor1, instructor1, "Session 3");

        _dbContext.Add(session1);
        _dbContext.Add(session2);
        _dbContext.Add(session3);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSessionsPagedQuery(new PaginationParameters { PageNumber = 1, PageSize = 10 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
        result.Data.Pagination.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithInstructorFilter_ReturnsOnlySessionsForThatInstructor()
    {
        // Arrange
        var targetInstructorId = Guid.NewGuid();
        var otherInstructorId = Guid.NewGuid();

        var session1 = Session.Create(targetInstructorId, targetInstructorId, "Target Session 1");
        var session2 = Session.Create(otherInstructorId, otherInstructorId, "Other Session");
        var session3 = Session.Create(targetInstructorId, targetInstructorId, "Target Session 2");

        _dbContext.Add(session1);
        _dbContext.Add(session2);
        _dbContext.Add(session3);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSessionsPagedQuery(
            new PaginationParameters { PageNumber = 1, PageSize = 10 },
            targetInstructorId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().OnlyContain(s => s.InstructorId == targetInstructorId);
        result.Data.Pagination.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_PaginationSkipAndTake_AppliesCorrectly()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        for (int i = 1; i <= 5; i++)
        {
            var session = Session.Create(instructorId, instructorId, $"Session {i}");
            _dbContext.Add(session);
        }
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSessionsPagedQuery(new PaginationParameters { PageNumber = 2, PageSize = 2 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Pagination.TotalCount.Should().Be(5);
        result.Data.Pagination.CurrentPage.Should().Be(2);
        result.Data.Pagination.PageSize.Should().Be(2);
    }
}
