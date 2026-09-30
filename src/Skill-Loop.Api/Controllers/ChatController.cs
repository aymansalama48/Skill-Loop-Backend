using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Chat;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Chat.Commands.MarkConversationRead;
using Skill_Loop.Application.Features.Chat.Commands.SendMessage;
using Skill_Loop.Application.Features.Chat.Commands.StartConversation;
using Skill_Loop.Application.Features.Chat.Queries.GetConversationMessages;
using Skill_Loop.Application.Features.Chat.Queries.GetMyConversations;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة المحادثات والرسائل بين المستخدمين
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class ChatController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public ChatController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    [HttpPost("conversations")]
    public async Task<IResult> StartConversation([FromBody] StartConversationRequest request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await Mediator.Send(new StartConversationCommand(userId, request.OtherUserId), cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("conversations")]
    public async Task<IResult> GetMyConversations(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await Mediator.Send(new GetMyConversationsQuery(userId), cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IResult> GetMessages(
        Guid conversationId,
        [FromQuery] GetConversationMessagesRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetConversationMessagesQuery
        {
            UserId = RequireUserId(),
            ConversationId = conversationId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("conversations/{conversationId:guid}/messages")]
    public async Task<IResult> SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await Mediator.Send(new SendMessageCommand(userId, conversationId, request.Content), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("conversations/{conversationId:guid}/read")]
    public async Task<IResult> MarkAsRead(Guid conversationId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await Mediator.Send(new MarkConversationReadCommand(userId, conversationId), cancellationToken);
        return HandleResult(result);
    }
}
