
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using Skill_Loop.Application.Features.Notifications.Commands.MarkNotificationRead;
using Skill_Loop.Application.Features.Notifications.Queries.GetMyNotifications;
using Skill_Loop.Application.Features.Notifications.Queries.GetUnreadNotificationCount;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة الإشعارات والتنبيهات للمستخدمين
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public NotificationsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    [HttpGet]
    public async Task<IResult> GetMyNotifications([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        var query = new GetMyNotificationsQuery
        {
            UserId = _currentUser.UserId ?? Guid.Empty,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("unread-count")]
    public async Task<IResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new GetUnreadNotificationCountQuery(userId), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("{notificationId:guid}/read")]
    public async Task<IResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new MarkNotificationReadCommand(userId, notificationId), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("read-all")]
    public async Task<IResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new MarkAllNotificationsReadCommand(userId), cancellationToken);
        return HandleResult(result);
    }
}
