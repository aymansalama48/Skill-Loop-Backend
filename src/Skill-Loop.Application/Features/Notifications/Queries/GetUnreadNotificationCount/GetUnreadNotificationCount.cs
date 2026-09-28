using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

/// <summary>
/// عدد الإشعارات غير المقروءة، عشان الرقم الأحمر الصغير فوق أيقونة الجرس.
/// </summary>
public sealed record GetUnreadNotificationCountQuery(Guid UserId) : IQuery<int>;

public sealed class GetUnreadNotificationCountQueryHandler : IQueryHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IApplicationDbContext _context;

    public GetUnreadNotificationCountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.CountAsync(
            _context.AsNoTracking(_context.Notifications).Where(n => n.UserId == request.UserId && !n.IsRead),
            cancellationToken);

        return Result<int>.Success(count);
    }
}
