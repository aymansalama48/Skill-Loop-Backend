using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Features.Chat.Commands.StartConversation;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat;
using Skill_Loop.UnitTests.Common;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Chat.Commands.StartConversation;

public class StartConversationCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IUserManagementService> _userManagement;
    private readonly StartConversationCommandHandler _handler;

    public StartConversationCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _userManagement = new Mock<IUserManagementService>();
        _handler = new StartConversationCommandHandler(_dbContext, _userManagement.Object);
    }

    [Fact]
    public async Task Handle_SameUserId_ReturnsCannotChatWithSelfError()
    {
        var userId = Guid.NewGuid();
        var command = new StartConversationCommand(userId, userId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ChatErrors.CannotChatWithSelf.Code);
    }

    [Fact]
    public async Task Handle_OtherUserNotFound_ReturnsNotFoundError()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var command = new StartConversationCommand(userId, otherUserId);

        _userManagement.Setup(u => u.GetByIdAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(new Error("User.NotFound", "User not found", ErrorType.NotFound)));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OtherUserInactive_ReturnsRecipientUnavailableError()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var command = new StartConversationCommand(userId, otherUserId);

        _userManagement.Setup(u => u.GetByIdAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = otherUserId, FullName = "Inactive User", Email = "inactive@example.com", AvatarUrl = null, IsActive = false }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ChatErrors.RecipientUnavailable.Code);
    }

    [Fact]
    public async Task Handle_NoExistingConversation_CreatesNewConversation()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var command = new StartConversationCommand(userId, otherUserId);

        _userManagement.Setup(u => u.GetByIdAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = otherUserId, FullName = "Other User", Email = "other@example.com", AvatarUrl = "avatar.png", IsActive = true }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.OtherUserId.Should().Be(otherUserId);
        result.Data.OtherUserName.Should().Be("Other User");
        result.Data.OtherUserAvatarUrl.Should().Be("avatar.png");
        result.Data.UnreadCount.Should().Be(0);

        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.Conversations, c => c.Id == result.Data.Id, CancellationToken.None);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ExistingConversation_ReturnsExistingConversation()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var existingConversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(existingConversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new StartConversationCommand(userId, otherUserId);

        _userManagement.Setup(u => u.GetByIdAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = otherUserId, FullName = "Other User", Email = "other@example.com", AvatarUrl = null, IsActive = true }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(existingConversation.Id);
    }

    [Fact]
    public async Task Handle_ExistingConversationWithUnreadMessages_ReturnsCorrectUnreadCount()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var existingConversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(existingConversation);

        // Add unread messages from other user
        var msg1 = ChatMessage.Create(existingConversation.Id, otherUserId, "Hello").Data!;
        var msg2 = ChatMessage.Create(existingConversation.Id, otherUserId, "How are you?").Data!;
        var msg3 = ChatMessage.Create(existingConversation.Id, userId, "I'm good").Data!;
        msg3.MarkAsRead();

        _dbContext.Add(msg1);
        _dbContext.Add(msg2);
        _dbContext.Add(msg3);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new StartConversationCommand(userId, otherUserId);

        _userManagement.Setup(u => u.GetByIdAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = otherUserId, FullName = "Other User", Email = "other@example.com", AvatarUrl = null, IsActive = true }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UnreadCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_UserIdOrderDoesNotMatter_ReturnsSameConversation()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var existingConversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(existingConversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Try to start from other user's perspective
        var command = new StartConversationCommand(otherUserId, userId);

        _userManagement.Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = userId, FullName = "User", Email = "user@example.com", AvatarUrl = null, IsActive = true }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(existingConversation.Id);
    }
}