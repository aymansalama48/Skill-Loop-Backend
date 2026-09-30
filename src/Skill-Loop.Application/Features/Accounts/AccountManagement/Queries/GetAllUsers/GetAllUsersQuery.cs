using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetAllUsers;

[Permission(Permissions.Users.View)]
public sealed record GetAllUsersQuery(
    int PageNumber,
    int PageSize,
    string? Role,
    string? SearchTerm
) : ICacheableQuery<PagedResult<UserDto>>
{
    public string CacheKey => $"users-list-{PageNumber}-{PageSize}-{Role ?? "all"}-{SearchTerm ?? "none"}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(2);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(10);
}