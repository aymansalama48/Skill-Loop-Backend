namespace Skill_Loop.Application.Features.Otps.Commands.RequestOtp;

using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;

public sealed record RequestOtpCommand(
    string PhoneNumber,
    OtpPurpose Purpose) : ICommand<RequestOtpResponse>;