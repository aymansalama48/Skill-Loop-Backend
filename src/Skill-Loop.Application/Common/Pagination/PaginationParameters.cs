namespace Skill_Loop.Application.Common.Pagination;

/// <summary>
/// يمثل بيانات الـ Pagination القادمة من الـ Client.
/// </summary>
public class PaginationParameters
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    public PaginationParameters()
    {
    }

    /// <summary>
    /// رقم الصفحة الحالية.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// عدد العناصر في الصفحة.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init
        {
            if (value < 1)
                _pageSize = DefaultPageSize;
            else
                _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    /// <summary>
    /// عدد العناصر التي سيتم تخطيها.
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}