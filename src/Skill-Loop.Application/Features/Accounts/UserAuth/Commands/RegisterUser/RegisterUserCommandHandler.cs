using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, bool>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IOtpService _otpService;
    private readonly IJobScheduler _jobScheduler;
    private readonly IClientContext _clientContext;

    public RegisterUserCommandHandler(
        IUserManagementService userManagementService,
        IOtpService otpService,
        IJobScheduler jobScheduler,
        IClientContext clientContext)
    {
        _userManagementService = userManagementService;
        _otpService = otpService;
        _jobScheduler = jobScheduler;
        _clientContext = clientContext;
    }

    public async Task<Result<bool>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. التحقق من وجود الإيميل مسبقاً
        var userExists = await _userManagementService.CheckUserExistsAsync(request.Email, cancellationToken);
        if (userExists.Data)
            return Result<bool>.Failure(UserErrors.EmailAlreadyExists);

        // 2. إنشاء المستخدم في قاعدة البيانات كغير مؤكد
        var createResult = await _userManagementService.CreateUserAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            cancellationToken);

        if (!createResult.IsSuccess)
            return Result<bool>.Failure(createResult.Errors);

        // 3. توليد كود الـ OTP
        var otpResult = await _otpService.GenerateOtpAsync(
            request.Email,
            OtpPurpose.EmailVerification,
            cancellationToken);

        if (!otpResult.IsSuccess)
            return Result<bool>.Failure(otpResult.Errors);

        // 4. تجهيز نموذج قالب البريد الإلكتروني بالبيانات الأساسية للعرض
        var templateModel = new EmailConfirmationTemplateModel
        {
            UserName = request.FirstName,
            UserEmail = request.Email,
            OtpCode = otpResult.Data!.Code
        };

        // 5. التقاط بيانات الاتصال من الـ HTTP Request الحالي
        var ipAddress = _clientContext.IpAddress;
        var userAgent = _clientContext.UserAgent;

        // 6. إرسال الكود في الخلفية (Background Job) مع تمرير المعاملات المنفصلة
        _jobScheduler.Enqueue<IIdentityNotificationService>(n =>
            n.SendEmailConfirmationAsync(
                request.Email,
                templateModel,
                ipAddress,
                userAgent));

        return Result<bool>.Success(true);
    }
}