using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetMyProfile;

public sealed class GetMyAccountProfileQueryHandler : IQueryHandler<GetMyAccountProfileQuery, MyAccountProfileResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserManagementService _userService;

    public GetMyAccountProfileQueryHandler(ICurrentUser currentUser, IUserManagementService userService)
    {
        _currentUser = currentUser;
        _userService = userService;
    }

    public async Task<Result<MyAccountProfileResponse>> Handle(GetMyAccountProfileQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result<MyAccountProfileResponse>.Failure(UserErrors.NotFound);
        }

        var userResult = await _userService.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<MyAccountProfileResponse>.Failure(UserErrors.NotFound);
        }

        var userDetails = userResult.Data;

        return Result<MyAccountProfileResponse>.Success(new MyAccountProfileResponse(
            userDetails!.FirstName,
            userDetails.LastName,
            userDetails.FullName,
            userDetails.Email,
            userDetails.PhoneNumber,
            userDetails.AvatarUrl
        ));
    }
}