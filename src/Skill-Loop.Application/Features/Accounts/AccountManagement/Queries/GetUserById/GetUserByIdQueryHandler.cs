using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;


namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserManagementService _userService;

    public GetUserByIdQueryHandler(IUserManagementService userService)
    {
        _userService = userService;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // الدالة GetByIdAsync بترجع Result<UserDto> جاهزة، بنعملها return مباشرة
        return await _userService.GetByIdAsync(request.UserId, cancellationToken);
    }
}