namespace Skill_Loop.Application.Features.Otps.Commands.RequestOtp;

public sealed record RequestOtpResponse(
    DateTime ExpiresAtUtc,
    DateTime NextResendAllowedAtUtc);