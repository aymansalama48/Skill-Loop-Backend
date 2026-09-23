using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;

public sealed class ResendEmailOtpCommandHandler : ICommandHandler<ResendEmailOtpCommand, bool>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IOtpService _otpService;
    private readonly IJobScheduler _jobScheduler;

    public ResendEmailOtpCommandHandler(
        IUserManagementService userManagementService,
        IOtpService otpService,
        IJobScheduler jobScheduler)
    {
        _userManagementService = userManagementService;
        _otpService = otpService;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result<bool>> Handle(ResendEmailOtpCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب بيانات المستخدم بالكامل
        var userResult = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);

        if (userResult.IsFailure)
            return Result<bool>.Failure(userResult.Errors);

        // 2. التحقق مما إذا كان الإيميل مؤكداً بالفعل (بافتراض أنك أضفت EmailConfirmed للـ Dto)
        // أو يمكنك استخدام الدالة القديمة IsEmailConfirmedAsync لو أردت
        if (userResult.Data!.EmailConfirmed)
            return Result<bool>.Failure(UserErrors.EmailAlreadyConfirmed);

        // 3. إعادة إرسال الكود
        var resendOtpResult = await _otpService.ResendOtpAsync(request.Email, OtpPurpose.EmailVerification, cancellationToken);

        if (resendOtpResult.IsFailure)
            return Result<bool>.Failure(resendOtpResult.Errors);

        // 4. جدولة الإيميل للإرسال (مع استخدام الاسم الحقيقي!) 🚀
        var templateModel = new EmailConfirmationTemplateModel
        {
            UserName = userResult.Data.FirstName, // 👈 الاسم الحقيقي من قاعدة البيانات
            UserEmail = request.Email, // 👈 الإيميل من الطلب نفسه
            OtpCode = resendOtpResult.Data.Code
        };

        _jobScheduler.Enqueue<IIdentityNotificationService>(n =>
            n.SendEmailConfirmationAsync(request.Email, templateModel));

        return Result<bool>.Success(true);
    }
}