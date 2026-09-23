using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.DeleteSessionMaterial;

public sealed record DeleteSessionMaterialCommand(
    Guid SessionId,
    Guid MaterialId) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        global::Skill_Loop.Application.Common.Constants.CacheKeys.SessionMaterials(SessionId),
        global::Skill_Loop.Application.Common.Constants.CacheKeys.SessionMaterial(MaterialId),
        global::Skill_Loop.Application.Common.Constants.CacheKeys.SessionMaterialsAll
    ];
}