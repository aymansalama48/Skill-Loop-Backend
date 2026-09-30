using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionDetails;

/// <summary>
/// تفاصيل الاستفسار كاملة (منها البريد واسم صاحب الاستفسار) — لفريق الدعم والـ Admin بس.
/// </summary>
[AllowAnonymous]
public sealed record GetSupportQuestionDetailsQuery(Guid Id) : IQuery<SupportQuestionDetailsResponse>
{
}
