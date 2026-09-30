
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffGoogleLogin;

[AuthenticatedOnly]
public sealed record StaffGoogleLoginCommand(string IdToken) : ICommand<StaffAuthResponse>;