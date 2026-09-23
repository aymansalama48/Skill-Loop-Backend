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
    private readonly IJobScheduler _jobScheduler;

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

        // 2. جلب بيانات المستخدم بالكامل (الإيميل والاسم) في استعلام واحد
        var userResult = await _userService.GetByIdAsync(request.UserId, cancellationToken);

        if (userResult.IsSuccess && !string.IsNullOrWhiteSpace(userResult.Data?.Email))
        {
            // 3. تجهيز الموديل مع تمرير الاسم الحقيقي للمستخدم
            var templateModel = new AccountUnlockedTemplateModel
            {
                UserName = userResult.Data.FirstName, // 👈 هنا تم استغلال الاستعلام لجلب الاسم
                UserEmail = userResult.Data.Email // 👈 هنا تم استغلال الاستعلام لجلب البريد الإلكتروني
            };

            var email = userResult.Data.Email;

            // 4. إرسال الإيميل في الخلفية (Fire and Forget) عبر Hangfire
            _jobScheduler.Enqueue<IIdentityNotificationService>(n =>
                n.SendAccountUnlockedEmailAsync(email, templateModel));
        }

        return Result<bool>.Success(true);
    }
}