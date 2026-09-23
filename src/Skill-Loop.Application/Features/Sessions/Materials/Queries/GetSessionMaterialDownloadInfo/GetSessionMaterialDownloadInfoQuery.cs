using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterialDownloadInfo;

public sealed record SessionMaterialDownloadInfoResponse(
    Guid Id,
    string FileName,
    string MimeType,
    long SizeBytes,
    string DriveFileId);

public sealed record GetSessionMaterialDownloadInfoQuery(
    Guid SessionId,
    Guid MaterialId) : ICacheableQuery<SessionMaterialDownloadInfoResponse>
{
    public string CacheKey => CacheKeys.SessionMaterialDownload(SessionId, MaterialId);

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);

    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}