namespace Skill_Loop.UnitTests.Features.Accounts.AccountManagement.Queries.GetAllUsers;

using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetAllUsers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserManagementService> _userService;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _userService = new Mock<IUserManagementService>();
        _handler = new GetAllUsersQueryHandler(_userService.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ReturnsSuccessWithPagedUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery(1, 10, null, null);
        var users = new List<UserDto>
        {
            new() { Id = Guid.NewGuid(), Email = "user1@example.com", FullName = "User One", PhoneNumber = "01000000001", IsActive = true, Roles = new() { "Student" }, AvatarUrl = "avatar1.png", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "user2@example.com", FullName = "User Two", PhoneNumber = "01000000002", IsActive = true, Roles = new() { "Instructor" }, AvatarUrl = "avatar2.png", CreatedAt = DateTime.UtcNow }
        };

        var pagedResult = new PagedResult<UserDto>
        {
            Items = users,
            Pagination = new PaginationMetadata
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 2
            }
        };

        _userService.Setup(s => s.GetAllUsersAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Pagination.TotalCount.Should().Be(2);
        result.Data.Pagination.CurrentPage.Should().Be(1);
        _userService.Verify(s => s.GetAllUsersAsync(1, 10, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRoleAndSearchFilters_PassesFiltersToUserService()
    {
        // Arrange
        var query = new GetAllUsersQuery(2, 5, "Instructor", "Ahmed");
        var pagedResult = new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            Pagination = new PaginationMetadata { CurrentPage = 2, PageSize = 5, TotalCount = 0 }
        };

        _userService.Setup(s => s.GetAllUsersAsync(2, 5, "Instructor", "Ahmed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userService.Verify(s => s.GetAllUsersAsync(2, 5, "Instructor", "Ahmed", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoUsersMatch_ReturnsSuccessWithEmptyPagedResult()
    {
        // Arrange
        var query = new GetAllUsersQuery(1, 10, "NonExistentRole", null);
        var emptyPagedResult = new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            Pagination = new PaginationMetadata { CurrentPage = 1, PageSize = 10, TotalCount = 0 }
        };

        _userService.Setup(s => s.GetAllUsersAsync(1, 10, "NonExistentRole", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenCancellationTokenProvided_ForwardsCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var query = new GetAllUsersQuery(1, 10, null, null);
        var pagedResult = new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            Pagination = new PaginationMetadata()
        };

        _userService.Setup(s => s.GetAllUsersAsync(1, 10, null, null, token))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userService.Verify(s => s.GetAllUsersAsync(1, 10, null, null, token), Times.Once);
    }
}
