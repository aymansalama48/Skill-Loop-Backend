using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Chat.Queries.GetConversationMessages;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat;
using Skill_Loop.UnitTests.Common;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Chat.Queries.GetConversationMessages;

public class GetConversationMessagesQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetConversationMessagesQueryHandler _handler;

    public GetConversationMessagesQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetConversationMessagesQueryHandler(_dbContext);
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
        var query = new GetConversationMessagesQuery
        {
            UserId = userId,
            ConversationId = conversationId,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

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
        var query = new GetConversationMessagesQuery
        {
            UserId = strangerId,
            ConversationId = conversation.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ChatErrors.NotParticipant.Code);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsPagedMessages()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var messages = new List<ChatMessage>();
        for (int i = 1; i <= 15; i++)
        {
            var msg = ChatMessage.Create(conversation.Id, i % 2 == 0 ? userId : otherUserId, $"Message {i}").Data!;
            SetSentAt(msg, DateTime.UtcNow.AddMinutes(i * -1)); // newer messages have smaller negative minutes
            messages.Add(msg);
        }
        _dbContext.AddRange(messages);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetConversationMessagesQuery
        {
            UserId = userId,
            ConversationId = conversation.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(10);
        result.Data.Pagination.CurrentPage.Should().Be(1);
        result.Data.Pagination.PageSize.Should().Be(10);
        result.Data.Pagination.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handle_OrdersBySentAtDescending()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var msg1 = ChatMessage.Create(conversation.Id, userId, "Oldest").Data!;
        SetSentAt(msg1, DateTime.UtcNow.AddHours(-2));
        var msg2 = ChatMessage.Create(conversation.Id, otherUserId, "Middle").Data!;
        SetSentAt(msg2, DateTime.UtcNow.AddHours(-1));
        var msg3 = ChatMessage.Create(conversation.Id, userId, "Newest").Data!;
        SetSentAt(msg3, DateTime.UtcNow.AddMinutes(-5));
        _dbContext.AddRange(new[] { msg1, msg2, msg3 });
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetConversationMessagesQuery
        {
            UserId = userId,
            ConversationId = conversation.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items[0].Content.Should().Be("Newest");
        result.Data.Items[1].Content.Should().Be("Middle");
        result.Data.Items[2].Content.Should().Be("Oldest");
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsCorrectItems()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var messages = new List<ChatMessage>();
        for (int i = 1; i <= 25; i++)
        {
            var msg = ChatMessage.Create(conversation.Id, i % 2 == 0 ? userId : otherUserId, $"Message {i}").Data!;
            SetSentAt(msg, DateTime.UtcNow.AddMinutes(i * -1));
            messages.Add(msg);
        }
        _dbContext.AddRange(messages);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetConversationMessagesQuery
        {
            UserId = userId,
            ConversationId = conversation.Id,
            PageNumber = 2,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(10);
        result.Data.Pagination.CurrentPage.Should().Be(2);
        result.Data.Pagination.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task Handle_PageSizeRespected()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conversation);

        for (int i = 1; i <= 5; i++)
        {
            var msg = ChatMessage.Create(conversation.Id, userId, $"Msg {i}").Data!;
            SetSentAt(msg, DateTime.UtcNow.AddMinutes(i * -1));
            _dbContext.Add(msg);
        }
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetConversationMessagesQuery
        {
            UserId = userId,
            ConversationId = conversation.Id,
            PageNumber = 1,
            PageSize = 3
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_MessageDtoHasCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var msg = ChatMessage.Create(conversation.Id, otherUserId, "Test content").Data!;
        SetSentAt(msg, DateTime.UtcNow);
        _dbContext.Add(msg);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetConversationMessagesQuery
        {
            UserId = userId,
            ConversationId = conversation.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Data!.Items.First();
        dto.Id.Should().Be(msg.Id);
        dto.ConversationId.Should().Be(conversation.Id);
        dto.SenderId.Should().Be(otherUserId);
        dto.Content.Should().Be("Test content");
        dto.SentAt.Should().BeCloseTo(msg.SentAt, TimeSpan.FromSeconds(1));
        dto.ReadAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithReadAt_ReturnsReadAtInDto()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, otherUserId).Data!;
        _dbContext.Add(conversation);

        var msg = ChatMessage.Create(conversation.Id, otherUserId, "Read message").Data!;
        SetSentAt(msg, DateTime.UtcNow.AddMinutes(-10));
        msg.MarkAsRead();
        _dbContext.Add(msg);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetConversationMessagesQuery
        {
            UserId = userId,
            ConversationId = conversation.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.First().ReadAt.Should().NotBeNull();
    }
}