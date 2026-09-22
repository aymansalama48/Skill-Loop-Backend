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

        // 2. جلب بيانات المستخدم بالكامل (الإيميل والاسم) في استعلام واحد
        var userResult = await _userService.GetByIdAsync(request.UserId, cancellationToken);

        if (userResult.IsSuccess && !string.IsNullOrWhiteSpace(userResult.Data?.Email))
        {
            // 3. تجهيز الموديل مع تمرير الاسم الحقيقي للمستخدم
            var templateModel = new AccountLockedTemplateModel
            {
                UserName = userResult.Data.FirstName, // 👈 جلب الاسم الحقيقي
                UserEmail = userResult.Data.Email         // 👈 تمرير الإيميل للموديل (تأكد أن اسم الخاصية Email وليس UserEmail بناءً على كلاس BaseEmailTemplateModel)
            };

            var email = userResult.Data.Email; // حفظ الإيميل في متغير عشان الـ Expression Tree الخاص بـ Hangfire يشتغل صح

            // 4. إرسال الإيميل في الخلفية (Fire and Forget) 🚀
            _jobScheduler.Enqueue<IIdentityNotificationService>(n =>
                n.SendAccountLockedEmailAsync(email, templateModel));
        }

        return Result<bool>.Success(true);
    }
}