using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserManagementService _userService;

    public GetAllUsersQueryHandler(IUserManagementService userService)
    {
        _userService = userService;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // 1. الدالة دلوقتي بترجع PagedResult بشكل مباشر (مفيش IsSuccess ولا Value)
        var pagedResult = await _userService.GetAllUsersAsync(
            request.PageNumber,
            request.PageSize,
            request.Role,
            request.SearchTerm,
            cancellationToken);

        // 2. بنغلف النتيجة مباشرة في Result.Success ونرجعها للكنترولر
        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }
}