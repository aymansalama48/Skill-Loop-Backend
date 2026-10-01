using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.DeleteSessionMaterial;

[Permission(Permissions.Sessions.DeleteMaterials)]
public sealed record DeleteSessionMaterialCommand(
    Guid SessionId,
    Guid MaterialId) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        global::Skill_Loop.Application.Common.Constants.AppCacheKeys.SessionMaterials(SessionId),
        global::Skill_Loop.Application.Common.Constants.AppCacheKeys.SessionMaterial(MaterialId),
        global::Skill_Loop.Application.Common.Constants.AppCacheKeys.SessionMaterialsAll
    ];
}
