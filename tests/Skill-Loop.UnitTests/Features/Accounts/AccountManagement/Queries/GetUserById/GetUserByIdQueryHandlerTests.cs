namespace Skill_Loop.UnitTests.Features.Accounts.AccountManagement.Queries.GetUserById;

using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetUserById;
using Skill_Loop.Domain.Common.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserManagementService> _userService;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userService = new Mock<IUserManagementService>();
        _handler = new GetUserByIdQueryHandler(_userService.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsSuccessWithUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var expectedUser = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User",
            PhoneNumber = "01000000000",
            IsActive = true,
            Roles = new() { "Student" },
            CreatedAt = DateTime.UtcNow
        };

        _userService.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(expectedUser));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.Email.Should().Be("test@example.com");
        _userService.Verify(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _userService.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(UserErrors.NotFound));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == UserErrors.NotFound.Code);
    }
}
