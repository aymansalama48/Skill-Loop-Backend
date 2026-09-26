using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Realtime;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Features.Chat.Commands.MarkConversationRead;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat;
using Skill_Loop.UnitTests.Common;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Chat.Commands.MarkConversationRead;

public class MarkConversationReadCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IChatNotifier> _notifier;
    private readonly MarkConversationReadCommandHandler _handler;

    public MarkConversationReadCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _notifier = new Mock<IChatNotifier>();
        _handler = new MarkConversationReadCommandHandler(_dbContext, _notifier.Object);
    }

    private static void SetSentAt(ChatMessage message, DateTime value)
    {
        typeof(ChatMessage).GetProperty("SentAt")!.SetValue(message, value);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ReturnsNotFoundError()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var command = new MarkConversationReadCommand(userId, conversationId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ChatErrors.ConversationNotFound.Code);
    }

    [Fact]
    public async Task Handle_UserNotParticipant_ReturnsForbiddenError()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var strangerId = Guid.NewGuid();
        var command = new MarkConversationReadCommand(strangerId, conversation.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ChatErrors.NotParticipant.Code);
    }

    [Fact]
    public async Task Handle_NoUnreadMessages_ReturnsSuccessWithoutNotification()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new MarkConversationReadCommand(senderId, conversation.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _notifier.Verify(n => n.NotifyMessagesReadAsync(It.IsAny<Guid>(), It.IsAny<MessagesReadDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnreadMessages_MarksAllAsRead()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var msg1 = ChatMessage.Create(conversation.Id, otherUserId, "Hello").Data!;
        var msg2 = ChatMessage.Create(conversation.Id, otherUserId, "How are you?").Data!;
        _dbContext.Add(msg1);
        _dbContext.Add(msg2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new MarkConversationReadCommand(senderId, conversation.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var unread = await _dbContext.ToListAsync(_dbContext.ChatMessages
            .Where(m => m.ConversationId == conversation.Id && m.SenderId != senderId && m.ReadAt == null), CancellationToken.None);
        unread.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithUnreadMessages_CallsNotifierWithReadAt()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var msg1 = ChatMessage.Create(conversation.Id, otherUserId, "Hello").Data!;
        var msg2 = ChatMessage.Create(conversation.Id, otherUserId, "How are you?").Data!;
        _dbContext.Add(msg1);
        _dbContext.Add(msg2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new MarkConversationReadCommand(senderId, conversation.Id);

        await _handler.Handle(command, CancellationToken.None);

        _notifier.Verify(n => n.NotifyMessagesReadAsync(
            otherUserId,
            It.Is<MessagesReadDto>(m => m.ConversationId == conversation.Id && m.ReaderId == senderId && m.ReadAt != default),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReadAtIsMaxOfAllReadMessages()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var baseTime = DateTime.UtcNow;
        var msg1 = ChatMessage.Create(conversation.Id, otherUserId, "Hello").Data!;
        SetSentAt(msg1, baseTime);
        var msg2 = ChatMessage.Create(conversation.Id, otherUserId, "How are you?").Data!;
        SetSentAt(msg2, baseTime.AddMinutes(5));
        _dbContext.Add(msg1);
        _dbContext.Add(msg2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new MarkConversationReadCommand(senderId, conversation.Id);

        await _handler.Handle(command, CancellationToken.None);

        var readMessages = await _dbContext.ToListAsync(_dbContext.ChatMessages
            .Where(m => m.ConversationId == conversation.Id && m.SenderId != senderId), CancellationToken.None);

        readMessages.All(m => m.ReadAt != null).Should().BeTrue();
        // ReadAt should be set to current time (after SentAt)
        readMessages.Max(m => m.ReadAt!.Value).Should().BeAfter(baseTime);
    }

    [Fact]
    public async Task Handle_MixedReadAndUnread_OnlyMarksUnread()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var msg1 = ChatMessage.Create(conversation.Id, otherUserId, "Old read").Data!;
        msg1.MarkAsRead();
        var msg2 = ChatMessage.Create(conversation.Id, otherUserId, "New unread").Data!;
        _dbContext.Add(msg1);
        _dbContext.Add(msg2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new MarkConversationReadCommand(senderId, conversation.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var allMessages = await _dbContext.ToListAsync(_dbContext.ChatMessages
            .Where(m => m.ConversationId == conversation.Id && m.SenderId != senderId), CancellationToken.None);

        allMessages.Should().HaveCount(2);
        allMessages.All(m => m.ReadAt != null).Should().BeTrue();
    }
}