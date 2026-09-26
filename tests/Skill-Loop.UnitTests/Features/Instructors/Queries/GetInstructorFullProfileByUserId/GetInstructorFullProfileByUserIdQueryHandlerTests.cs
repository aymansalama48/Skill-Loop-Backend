using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;
using Skill_Loop.Application.Features.Instructors.Share;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Instructors.Queries.GetInstructorFullProfileByUserId;

public class GetInstructorFullProfileByUserIdQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IUserManagementService> _userService;
    private readonly GetInstructorFullProfileByUserIdQueryHandler _handler;

    public GetInstructorFullProfileByUserIdQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _userService = new Mock<IUserManagementService>();
        _handler = new GetInstructorFullProfileByUserIdQueryHandler(_dbContext, _userService.Object);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();
        var query = new GetInstructorFullProfileByUserIdQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        profile.Approve();
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(new Error("User.NotFound", "not found", ErrorType.NotFound)));

        var query = new GetInstructorFullProfileByUserIdQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenProfileAndUserExist_ReturnsFullProfileResponse()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Senior .NET Developer", "Experienced instructor.").Data!;
        profile.Approve();
        profile.AddReview(Guid.NewGuid(), 5, "Excellent");
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                FullName = "John Doe",
                AvatarUrl = "https://cdn.skillloop.com/avatar.png"
            }));

        var query = new GetInstructorFullProfileByUserIdQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);
        result.Data.ProfileId.Should().Be(profile.Id);
        result.Data.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
        result.Data.FullName.Should().Be("John Doe");
        result.Data.AvatarUrl.Should().Be("https://cdn.skillloop.com/avatar.png");
        result.Data.Headline.Should().Be("Senior .NET Developer");
        result.Data.Bio.Should().Be("Experienced instructor.");
        result.Data.IsApproved.Should().BeTrue();
        result.Data.Rating.Should().Be(5.0);
        result.Data.SessionsCompleted.Should().Be(0);
        result.Data.CreditsEarned.Should().Be(0);
    }
}
