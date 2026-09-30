using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterialDownloadInfo;

public sealed record SessionMaterialDownloadInfoResponse(
    Guid Id,
    string FileName,
    string MimeType,
    long SizeBytes,
    string DriveFileId);

[AuthenticatedOnly]
public sealed record GetSessionMaterialDownloadInfoQuery(
    Guid SessionId,
    Guid MaterialId) : IQuery<SessionMaterialDownloadInfoResponse>;