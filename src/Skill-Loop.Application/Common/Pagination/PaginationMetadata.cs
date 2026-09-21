namespace Skill_Loop.Application.Common.Pagination;

/// <summary>
/// معلومات الـ Pagination الخاصة بالاستجابة.
/// </summary>
public sealed class PaginationMetadata
{
    public int CurrentPage { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}