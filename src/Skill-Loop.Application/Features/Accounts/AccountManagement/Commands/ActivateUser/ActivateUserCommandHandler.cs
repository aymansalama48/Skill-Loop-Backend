using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ActivateUser;

public sealed class ActivateUserCommandHandler : ICommandHandler<ActivateUserCommand, bool>
{
    private readonly IUserManagementService _userService;
    private readonly IJobScheduler _jobScheduler; // 👈 استخدام الـ Scheduler

    public ActivateUserCommandHandler(
        IUserManagementService userService,
        IJobScheduler jobScheduler)
    {
        _userService = userService;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result<bool>> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. تنفيذ عملية التفعيل
        var result = await _userService.ActivateUserAsync(request.UserId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(result.Errors);
        }

        // 2. جلب بيانات المستخدم عشان ناخد الإيميل بتاعه
        var userResult = await _userService.GetByIdAsync(request.UserId, cancellationToken);

        if (userResult.IsSuccess && !string.IsNullOrWhiteSpace(userResult.Data.Email))
        {
            var templateModel = new AccountUnlockedTemplateModel();
            var email = userResult.Data.Email;

            // 3. إرسال الإيميل في الخلفية (Fire and Forget) 🚀
            _jobScheduler.Enqueue<IIdentityNotificationService>(n =>
                n.SendAccountUnlockedEmailAsync(email, templateModel));
        }

        return Result<bool>.Success(true);
    }
}