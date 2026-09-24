using Microsoft.AspNetCore.SignalR;
using Skill_Loop.Application.Features.Chat.Commands.MarkConversationRead;
using Skill_Loop.Application.Features.Chat.Commands.SendMessage;
using Skill_Loop.Application.Features.Chat.DTOs;

namespace Skill_Loop.Api.Hubs;

/// <summary>
/// نقطة الاتصال اللحظي (WebSocket). الـ Hub ده رفيع جداً: ما فيهوش منطق أعمال،
/// بيحوّل الاستدعاء لـ Command عن طريق MediatR بالظبط زي الـ Controller.
/// </summary>
[Authorize]
public sealed class ChatHub(ISender mediator, ILogger<ChatHub> logger) : Hub
{
    public const string Route = "/hubs/chat";

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Chat connected. User={UserId} Connection={ConnectionId}",
            Context.UserIdentifier, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// الـ Client بينادي: connection.invoke("SendMessage", conversationId, "أهلاً")
    /// بيرجع الرسالة بعد ما اتحفظت، أو بيرمي HubException لو فيه خطأ.
    /// </summary>
    public async Task<MessageDto> SendMessage(Guid conversationId, string content)
    {
        var result = await mediator.Send(
            new SendMessageCommand(GetUserId(), conversationId, content),
            Context.ConnectionAborted);

        if (result.IsFailure)
            throw new HubException(FormatErrors(result));

        return result.Data!;
    }

    /// <summary>
    /// الـ Client بينادي: connection.invoke("MarkAsRead", conversationId)
    /// </summary>
    public async Task MarkAsRead(Guid conversationId)
    {
        var result = await mediator.Send(
            new MarkConversationReadCommand(GetUserId(), conversationId),
            Context.ConnectionAborted);

        if (result.IsFailure)
            throw new HubException(FormatErrors(result));
    }

    private Guid GetUserId()
        => Guid.TryParse(Context.UserIdentifier, out var id)
            ? id
            : throw new HubException("Unauthorized.");

    private static string FormatErrors(Skill_Loop.Domain.Common.Results.Result result)
        => string.Join(" | ", result.Errors.Select(e => e.Description));
}
