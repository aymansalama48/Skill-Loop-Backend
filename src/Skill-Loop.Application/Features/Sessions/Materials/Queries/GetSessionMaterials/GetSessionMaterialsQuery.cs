using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterials;

public sealed record SessionMaterialResponse(
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

[AuthenticatedOnly]
public sealed record GetSessionMaterialsQuery(
    Guid SessionId,
    PaginationParameters Pagination) : ICacheableQuery<PagedResult<SessionMaterialResponse>>
{
    public string CacheKey => CacheKeys.SessionMaterialsPaged(SessionId, Pagination.PageNumber, Pagination.PageSize);

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);

    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}