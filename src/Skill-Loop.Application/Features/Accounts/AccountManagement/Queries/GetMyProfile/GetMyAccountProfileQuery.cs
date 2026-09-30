using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetMyProfile;

[AuthenticatedOnly]
public sealed record GetMyAccountProfileQuery() : IQuery<MyAccountProfileResponse>;