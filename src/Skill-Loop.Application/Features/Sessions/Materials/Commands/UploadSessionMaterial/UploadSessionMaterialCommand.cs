using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.UploadSessionMaterial;

public sealed record UploadSessionMaterialCommand(
    Guid SessionId,
    Stream FileStream,
    string FileName,
    string MimeType,
    long SizeBytes,
    Skill_Loop.Domain.Enums.MaterialType? MaterialType = null) : ICommand<UploadSessionMaterialResponse>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        global::Skill_Loop.Application.Common.Constants.CacheKeys.SessionMaterials(SessionId),
        global::Skill_Loop.Application.Common.Constants.CacheKeys.SessionMaterialsAll
    ];
}

public sealed record UploadSessionMaterialResponse(
    Guid Id,
    Guid SessionId,
    string FileName,
    string MimeType,
    long SizeBytes,
    string DriveFileId,
    string? DriveFolderId,
    int SortOrder,
    Skill_Loop.Domain.Enums.MaterialType? MaterialType,
    DateTime CreatedAt);