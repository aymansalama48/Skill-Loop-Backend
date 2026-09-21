
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;

namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffGoogleLogin;

public sealed record StaffGoogleLoginCommand(string IdToken) : ICommand<StaffAuthResponse>;