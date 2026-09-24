using Microsoft.AspNetCore.SignalR;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Api.Hubs;

/// <summary>
/// SignalR بيدوّر افتراضياً على Claim اسمه NameIdentifier عشان يعرف "الـ User ده مين".
/// التوكن بتاعنا بيحط الـ Id في Claim اسمه "userId" (CustomClaims.UserId)،
/// فبنعلّم SignalR يقراه من هناك. من غير الكلاس ده، Clients.User(id) مش هيلاقي حد.
/// </summary>
public sealed class ChatUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(CustomClaims.UserId)?.Value;
}
