using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorProfileByUserId;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Instructors.Queries.GetInstructorProfileByUserId;

public class GetInstructorProfileByUserIdQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetInstructorProfileByUserIdQueryHandler _handler;

    public GetInstructorProfileByUserIdQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetInstructorProfileByUserIdQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();
        var query = new GetInstructorProfileByUserIdQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ReturnsProfileResponse()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Senior Developer", "Experienced").Data!;
        profile.Approve();
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetInstructorProfileByUserIdQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(profile.Id);
        result.Data.UserId.Should().Be(userId);
        result.Data.Headline.Should().Be("Senior Developer");
        result.Data.Bio.Should().Be("Experienced");
        result.Data.IsApproved.Should().BeTrue();
    }
}
