using Skill_Loop.Api.Contracts.Common;

namespace Skill_Loop.Api.Contracts.Users;

public record GetUsersRequest : PaginationRequest
{
    public string? Role { get; init; }
    public string? SearchTerm { get; init; }
}