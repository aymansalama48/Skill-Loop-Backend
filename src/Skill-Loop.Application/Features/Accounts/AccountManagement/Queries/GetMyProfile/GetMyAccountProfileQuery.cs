using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetMyProfile;

public sealed record GetMyAccountProfileQuery() : IQuery<MyAccountProfileResponse>;