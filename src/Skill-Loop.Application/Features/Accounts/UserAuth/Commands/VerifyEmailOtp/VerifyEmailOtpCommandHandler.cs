using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

public sealed class VerifyEmailOtpCommandHandler : ICommandHandler<VerifyEmailOtpCommand, bool>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IOtpService _otpService;

    public VerifyEmailOtpCommandHandler(
        IUserManagementService userManagementService,
        IOtpService otpService)
    {
        _userManagementService = userManagementService;
        _otpService = otpService;
    }

    public async Task<Result<bool>> Handle(VerifyEmailOtpCommand request, CancellationToken cancellationToken)
    {
        // 1. التحقق من صحة الكود عبر خدمة الـ OTP
        var otpValidationResult = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode, OtpPurpose.EmailVerification, cancellationToken);

        if (otpValidationResult.IsFailure)
            return Result<bool>.Failure(otpValidationResult.Errors);

        // 2. تفعيل الإيميل عبر خدمة إدارة المستخدمين
        var confirmResult = await _userManagementService.ConfirmUserEmailAsync(request.Email, cancellationToken);

        if (confirmResult.IsFailure)
            return Result<bool>.Failure(confirmResult.Errors);

        return Result<bool>.Success(true);
    }
}