using MediatR;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Common;

/// <summary>
/// واجهة مشتركة للـ Commands الخاصة بـ Session لتمكين التحقق من الملكية
/// </summary>
public interface ISessionCommand : IRequest<Skill_Loop.Domain.Common.Results.Result>
{
    public Guid SessionId { get; }
}
