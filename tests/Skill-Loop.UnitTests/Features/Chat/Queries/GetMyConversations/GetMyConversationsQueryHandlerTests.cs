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
using Skill_Loop.Application.Features.Chat.Queries.GetMyConversations;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat;
using Skill_Loop.UnitTests.Common;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Chat.Queries.GetMyConversations;

public class GetMyConversationsQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IUserManagementService> _userManagement;
    private readonly GetMyConversationsQueryHandler _handler;

    public GetMyConversationsQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _userManagement = new Mock<IUserManagementService>();
        _handler = new GetMyConversationsQueryHandler(_dbContext, _userManagement.Object);
    }

    [Fact]
    public async Task Handle_NoConversations_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        var query = new GetMyConversationsQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithConversations_ReturnsListWithCorrectData()
    {
        var userId = Guid.NewGuid();
        var otherUserId1 = Guid.NewGuid();
        var otherUserId2 = Guid.NewGuid();

        var conv1 = Conversation.Create(userId, otherUserId1).Data!;
        var conv2 = Conversation.Create(userId, otherUserId2).Data!;
        _dbContext.Add(conv1);
        _dbContext.Add(conv2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetMyConversationsQuery(userId);

        _userManagement.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>
            {
                new UserDto { Id = otherUserId1, FullName = "User One", Email = "one@example.com", AvatarUrl = "avatar1.png", IsActive = true },
                new UserDto { Id = otherUserId2, FullName = "User Two", Email = "two@example.com", AvatarUrl = "avatar2.png", IsActive = true }
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_OrdersByLastMessageAtDescending()
    {
        var userId = Guid.NewGuid();
        var otherUserId1 = Guid.NewGuid();
        var otherUserId2 = Guid.NewGuid();

        var conv1 = Conversation.Create(userId, otherUserId1).Data!;
        var conv2 = Conversation.Create(userId, otherUserId2).Data!;
        _dbContext.Add(conv1);
        _dbContext.Add(conv2);

        // conv1 has older last message
        var msg1 = ChatMessage.Create(conv1.Id, otherUserId1, "Old message").Data!;
        // Use reflection to set SentAt for testing
        typeof(ChatMessage).GetProperty("SentAt")!.SetValue(msg1, DateTime.UtcNow.AddHours(-2));
        _dbContext.Add(msg1);
        conv1.RegisterMessage(msg1);

        // conv2 has newer last message
        var msg2 = ChatMessage.Create(conv2.Id, otherUserId2, "New message").Data!;
        typeof(ChatMessage).GetProperty("SentAt")!.SetValue(msg2, DateTime.UtcNow.AddMinutes(-10));
        _dbContext.Add(msg2);
        conv2.RegisterMessage(msg2);

        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetMyConversationsQuery(userId);

        _userManagement.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>
            {
                new UserDto { Id = otherUserId1, FullName = "User One", Email = "one@example.com", AvatarUrl = null, IsActive = true },
                new UserDto { Id = otherUserId2, FullName = "User Two", Email = "two@example.com", AvatarUrl = null, IsActive = true }
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].Id.Should().Be(conv2.Id); // conv2 first (newer)
        result.Data[1].Id.Should().Be(conv1.Id);
    }

    [Fact]
    public async Task Handle_CalculatesUnreadCountCorrectly()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var conv = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conv);

        // 3 unread messages from other user
        _dbContext.Add(ChatMessage.Create(conv.Id, otherUserId, "Msg 1").Data!);
        _dbContext.Add(ChatMessage.Create(conv.Id, otherUserId, "Msg 2").Data!);
        _dbContext.Add(ChatMessage.Create(conv.Id, otherUserId, "Msg 3").Data!);
        // 1 read message from other user
        var readMsg = ChatMessage.Create(conv.Id, otherUserId, "Read msg").Data!;
        readMsg.MarkAsRead();
        _dbContext.Add(readMsg);
        // 2 messages from current user
        _dbContext.Add(ChatMessage.Create(conv.Id, userId, "My msg 1").Data!);
        _dbContext.Add(ChatMessage.Create(conv.Id, userId, "My msg 2").Data!);

        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetMyConversationsQuery(userId);

        _userManagement.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>
            {
                new UserDto { Id = otherUserId, FullName = "Other User", Email = "other@example.com", AvatarUrl = null, IsActive = true }
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(1);
        result.Data[0].UnreadCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_AsParticipantTwo_ReturnsConversation()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        // User is ParticipantTwo
        var conv = Conversation.Create(otherUserId, userId).Data!;
        _dbContext.Add(conv);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetMyConversationsQuery(userId);

        _userManagement.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>
            {
                new UserDto { Id = otherUserId, FullName = "Other User", Email = "other@example.com", AvatarUrl = null, IsActive = true }
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(1);
        result.Data[0].OtherUserId.Should().Be(otherUserId);
    }

    [Fact]
    public async Task Handle_UserNotFound_UsesFallbackName()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var conv = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conv);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetMyConversationsQuery(userId);

        // Return empty user list (user not found)
        _userManagement.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(1);
        result.Data[0].OtherUserName.Should().Be("مستخدم غير معروف");
    }
}