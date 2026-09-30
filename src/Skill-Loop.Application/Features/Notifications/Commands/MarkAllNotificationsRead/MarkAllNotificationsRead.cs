using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

/// <summary>
/// زرار "علّم الكل كمقروء" في شاشة الإشعارات.
/// </summary>
[AuthenticatedOnly]
public sealed record MarkAllNotificationsReadCommand(Guid UserId) : ICommand;

public sealed class MarkAllNotificationsReadCommandHandler : ICommandHandler<MarkAllNotificationsReadCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkAllNotificationsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _context.ToListAsync(
            _context.Notifications.Where(n => n.UserId == request.UserId && !n.IsRead),
            cancellationToken);

        if (unread.Count == 0)
            return Result.Success();

        foreach (var notification in unread)
            notification.MarkAsRead();

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
