using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Realtime;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Features.Chat.Commands.SendMessage;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat;
using Skill_Loop.UnitTests.Common;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Chat.Commands.SendMessage;

public class SendMessageCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IChatNotifier> _notifier;
    private readonly SendMessageCommandHandler _handler;

    public SendMessageCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _notifier = new Mock<IChatNotifier>();
        _handler = new SendMessageCommandHandler(_dbContext, _notifier.Object);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ReturnsNotFoundError()
    {
        var senderId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var command = new SendMessageCommand(senderId, conversationId, "Hello");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ChatErrors.ConversationNotFound.Code);
    }

    [Fact]
    public async Task Handle_SenderNotParticipant_ReturnsForbiddenError()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var strangerId = Guid.NewGuid();
        var command = new SendMessageCommand(strangerId, conversation.Id, "Hello");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ChatErrors.NotParticipant.Code);
    }

    [Fact]
    public async Task Handle_EmptyContent_ReturnsValidationError()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new SendMessageCommand(senderId, conversation.Id, "");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ContentTooLong_ReturnsValidationError()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var longContent = new string('a', ChatMessage.MaxContentLength + 1);
        var command = new SendMessageCommand(senderId, conversation.Id, longContent);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidMessage_CreatesMessageAndReturnsDto()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new SendMessageCommand(senderId, conversation.Id, "Hello, world!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Content.Should().Be("Hello, world!");
        result.Data.SenderId.Should().Be(senderId);
        result.Data.ConversationId.Should().Be(conversation.Id);
        result.Data.SentAt.Should().NotBe(default);
        result.Data.ReadAt.Should().BeNull();

        var stored = await _dbContext.FirstOrDefaultAsync(_dbContext.ChatMessages, m => m.ConversationId == conversation.Id, CancellationToken.None);
        stored.Should().NotBeNull();
        stored!.Content.Should().Be("Hello, world!");
    }

    [Fact]
    public async Task Handle_ValidMessage_UpdatesConversationLastMessage()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new SendMessageCommand(senderId, conversation.Id, "New message");

        await _handler.Handle(command, CancellationToken.None);

        var updated = await _dbContext.FirstOrDefaultAsync(_dbContext.Conversations, c => c.Id == conversation.Id, CancellationToken.None);
        updated!.LastMessagePreview.Should().Be("New message");
        updated.LastMessageAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidMessage_CallsNotifierForRecipient()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new SendMessageCommand(senderId, conversation.Id, "Hello");

        await _handler.Handle(command, CancellationToken.None);

        _notifier.Verify(n => n.NotifyMessageReceivedAsync(
            otherUserId,
            It.Is<MessageDto>(m => m.Content == "Hello" && m.SenderId == senderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MessageFromOtherUser_CallsNotifierWithCorrectRecipient()
    {
        var senderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(senderId, otherUserId).Data!;
        _dbContext.Add(conversation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new SendMessageCommand(otherUserId, conversation.Id, "Reply from other");

        await _handler.Handle(command, CancellationToken.None);

        _notifier.Verify(n => n.NotifyMessageReceivedAsync(
            senderId,
            It.Is<MessageDto>(m => m.Content == "Reply from other" && m.SenderId == otherUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}