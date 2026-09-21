using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand, bool>
{
    private readonly IUserManagementService _userService;
    private readonly IJobScheduler _jobScheduler;

    public AssignRoleToUserCommandHandler(IUserManagementService userService, IJobScheduler jobScheduler)
    {
        _userService = userService;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result<bool>> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _userService.AssignRoleAsync(request.UserId, request.RoleName, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(result.Errors);
        }

        var userResult = await _userService.GetByIdAsync(request.UserId, cancellationToken);
        if (userResult.IsSuccess && !string.IsNullOrWhiteSpace(userResult.Data.Email))
        {
            var templateModel = new RoleAssignedTemplateModel
            {
                RoleName = request.RoleName
            };
            var email = userResult.Data.Email;

            // إرسال الإيميل في الخلفية
            _jobScheduler.Enqueue<IIdentityNotificationService>(n =>
                n.SendRoleAssignedEmailAsync(email, templateModel));
        }

        return Result<bool>.Success(true);
    }
}