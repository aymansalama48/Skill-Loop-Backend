using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Pagination;

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

public sealed record GetSessionMaterialsQuery(
    Guid SessionId,
    PaginationParameters Pagination) : ICacheableQuery<PagedResult<SessionMaterialResponse>>
{
    public string CacheKey => CacheKeys.SessionMaterialsPaged(SessionId, Pagination.PageNumber, Pagination.PageSize);

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);

    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}