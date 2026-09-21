using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.DeactivateUser;


public sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand, bool>
{
    private readonly IUserManagementService _userService;
    private readonly IJobScheduler _jobScheduler; 

    public DeactivateUserCommandHandler(
        IUserManagementService userService,
        IJobScheduler jobScheduler)
    {
        _userService = userService;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result<bool>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. تنفيذ عملية التعطيل
        var result = await _userService.DeactivateUserAsync(request.UserId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(result.Errors);
        }

        // 2. جلب بيانات المستخدم عشان ناخد الإيميل بتاعه
        var userResult = await _userService.GetByIdAsync(request.UserId, cancellationToken);

        if (userResult.IsSuccess && !string.IsNullOrWhiteSpace(userResult.Data.Email))
        {
            var templateModel = new AccountLockedTemplateModel();
            var email = userResult.Data.Email; // حفظ الإيميل في متغير عشان الـ Expression Tree

            // 3. إرسال الإيميل في الخلفية (Fire and Forget) 🚀
            _jobScheduler.Enqueue<IIdentityNotificationService>(n =>
                n.SendAccountLockedEmailAsync(email, templateModel));
        }

        return Result<bool>.Success(true);
    }
}