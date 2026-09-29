
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using Skill_Loop.Application.Features.Notifications.Commands.MarkNotificationRead;
using Skill_Loop.Application.Features.Notifications.Queries.GetMyNotifications;
using Skill_Loop.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

namespace Skill_Loop.Api.Controllers;

/// <summary>
/// إشعارات جوه التطبيق (In-App). دي بتتعمل حالياً لما حد يبعتلك رسالة شات،
/// وممكن تتوسّع بعدين لأي حدث تاني (حجز جديد، دفع... إلخ) من غير أي تغيير هنا.
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

    /// <summary>قايمة إشعاراتي، الأحدث فوق.</summary>
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

    /// <summary>عدد الإشعارات غير المقروءة (للرقم الأحمر فوق أيقونة الجرس).</summary>
    [HttpGet("unread-count")]
    public async Task<IResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new GetUnreadNotificationCountQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>تعليم إشعار واحد كمقروء (لما المستخدم يدوس عليه).</summary>
    [HttpPost("{notificationId:guid}/read")]
    public async Task<IResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new MarkNotificationReadCommand(userId, notificationId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>تعليم كل الإشعارات كمقروءة (زرار "علّم الكل").</summary>
    [HttpPost("read-all")]
    public async Task<IResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new MarkAllNotificationsReadCommand(userId), cancellationToken);
        return HandleResult(result);
    }
}
