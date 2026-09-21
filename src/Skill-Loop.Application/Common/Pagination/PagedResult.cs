namespace Skill_Loop.Application.Common.Pagination;

/// <summary>
/// يمثل بيانات مقسمة على صفحات.
/// </summary>
public sealed class PagedResult<T>
{
    /// <summary>
    /// العناصر الموجودة فى الصفحة الحالية.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// معلومات الصفحات.
    /// </summary>
    public PaginationMetadata Pagination { get; init; } = null!;
}