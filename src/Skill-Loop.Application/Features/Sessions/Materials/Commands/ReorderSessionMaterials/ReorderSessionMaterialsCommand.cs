using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.ReorderSessionMaterials;

[AuthenticatedOnly]
public sealed record ReorderSessionMaterialsCommand(
    Guid SessionId,
    IReadOnlyList<Guid> OrderedMaterialIds) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        global::Skill_Loop.Application.Common.Constants.CacheKeys.SessionMaterials(SessionId),
        global::Skill_Loop.Application.Common.Constants.CacheKeys.SessionMaterialsAll
    ];
}