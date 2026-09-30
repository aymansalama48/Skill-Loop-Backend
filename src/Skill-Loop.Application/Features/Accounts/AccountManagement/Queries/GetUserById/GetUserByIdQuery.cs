using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetUserById;

[AuthenticatedOnly]
public sealed record GetUserByIdQuery(Guid UserId) : ICacheableQuery<UserDto>
{
    public string CacheKey => $"user-details-{UserId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(2);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(10);
}