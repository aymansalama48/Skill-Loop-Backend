using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;
using Skill_Loop.Application.Features.Instructors.Share;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Instructors.Queries.GetInstructorsPaged;

public class GetInstructorsPagedQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IUserManagementService> _userService;
    private readonly GetInstructorsPagedQueryHandler _handler;

    public GetInstructorsPagedQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _userService = new Mock<IUserManagementService>();
        _handler = new GetInstructorsPagedQueryHandler(_dbContext, _userService.Object);
    }

    [Fact]
    public async Task Handle_WhenNoApprovedInstructors_ReturnsEmptyPagedResult()
    {
        var query = new GetInstructorsPagedQuery(1, 10, null, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
        result.Data.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_OnlyApprovedInstructorsIncluded()
    {
        var approvedUser = Guid.NewGuid();
        var pendingUser = Guid.NewGuid();

        var approved = InstructorProfile.Create(approvedUser, "A1", "B").Data!;
        approved.Approve();

        var pending = InstructorProfile.Create(pendingUser, "A2", "B").Data!;

        _dbContext.Add(approved);
        _dbContext.Add(pending);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(approvedUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto
            {
                Id = approvedUser,
                FullName = "Approved User",
                AvatarUrl = "avatar.png"
            }));

        var query = new GetInstructorsPagedQuery(1, 10, null, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.Single().UserId.Should().Be(approvedUser);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersByHeadlineAndBio()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var profile1 = InstructorProfile.Create(userId1, "React Developer", "Bio about React").Data!;
        profile1.Approve();
        var profile2 = InstructorProfile.Create(userId2, "Angular Developer", "Bio about Angular").Data!;
        profile2.Approve();

        _dbContext.Add(profile1);
        _dbContext.Add(profile2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = userId1, FullName = "User1" }));

        var query = new GetInstructorsPagedQuery(1, 10, "react", null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.Single().UserId.Should().Be(userId1);
    }

    [Fact]
    public async Task Handle_WithMinRating_FiltersByMinimumRating()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var profile1 = InstructorProfile.Create(userId1, "H1", "B").Data!;
        profile1.Approve();
        profile1.AddReview(Guid.NewGuid(), 5, "Great");

        var profile2 = InstructorProfile.Create(userId2, "H2", "B").Data!;
        profile2.Approve();
        profile2.AddReview(Guid.NewGuid(), 3, "Okay");

        _dbContext.Add(profile1);
        _dbContext.Add(profile2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = userId1, FullName = "User1" }));
        _userService.Setup(s => s.GetByIdAsync(userId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = userId2, FullName = "User2" }));

        var query = new GetInstructorsPagedQuery(1, 10, null, 4, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.Single().UserId.Should().Be(userId1);
    }

    [Fact]
    public async Task Handle_WithHasCompletedSessionsFiltersBySessionsCompleted()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var profile1 = InstructorProfile.Create(userId1, "H1", "B").Data!;
        profile1.Approve();
        profile1.IncrementSessionsCompleted();

        var profile2 = InstructorProfile.Create(userId2, "H2", "B").Data!;
        profile2.Approve();

        _dbContext.Add(profile1);
        _dbContext.Add(profile2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = userId1, FullName = "User1" }));

        var query = new GetInstructorsPagedQuery(1, 10, null, null, true, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.Single().UserId.Should().Be(userId1);
    }

    [Fact]
    public async Task Handle_WithPagination_SkipsAndTakesCorrectly()
    {
        for (int i = 0; i < 5; i++)
        {
            var profile = InstructorProfile.Create(Guid.NewGuid(), $"H{i}", "B").Data!;
            profile.Approve();
            _dbContext.Add(profile);
        }
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => Result<UserDto>.Success(new UserDto { Id = id, FullName = "User" }));

        var query = new GetInstructorsPagedQuery(2, 2, null, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Pagination.CurrentPage.Should().Be(2);
        result.Data.Pagination.PageSize.Should().Be(2);
        result.Data.Pagination.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenUserServiceFailsForUser_SkipsThatUser()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "H", "B").Data!;
        profile.Approve();
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(new Error("User.NotFound", "not found", ErrorType.NotFound)));

        var query = new GetInstructorsPagedQuery(1, 10, null, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DefaultSort_SortsByRatingDescending()
    {
        var lowUser = Guid.NewGuid();
        var highUser = Guid.NewGuid();

        var lowProfile = InstructorProfile.Create(lowUser, "Low", "B").Data!;
        lowProfile.Approve();
        lowProfile.AddReview(Guid.NewGuid(), 2, "Bad");

        var highProfile = InstructorProfile.Create(highUser, "High", "B").Data!;
        highProfile.Approve();
        highProfile.AddReview(Guid.NewGuid(), 5, "Great");

        _dbContext.Add(lowProfile);
        _dbContext.Add(highProfile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _userService.Setup(s => s.GetByIdAsync(highUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = highUser, FullName = "High" }));
        _userService.Setup(s => s.GetByIdAsync(lowUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = lowUser, FullName = "Low" }));

        var query = new GetInstructorsPagedQuery(1, 10, null, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.First().UserId.Should().Be(highUser);
    }
}
