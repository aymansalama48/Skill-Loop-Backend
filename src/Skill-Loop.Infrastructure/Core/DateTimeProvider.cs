using Skill_Loop.Application.Common.Abstractions.Core;

namespace Skill_Loop.Infrastructure.Core;

public class DateTimeProvider : IDateTime
{
    private static readonly TimeZoneInfo EgyptTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

    /// <summary>
    /// 🔹 1. الوقت الحالي بتوقيت UTC — استخدمه للتخزين والمقارنات والتوكنات
    /// </summary>
    public System.DateTime UtcNow => System.DateTime.UtcNow;

    /// <summary>
    /// 🔹 2. الوقت الحالي بتوقيت العرض (مصر) — استخدمه للعرض فقط
    /// </summary>
    public System.DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(System.DateTime.UtcNow, EgyptTimeZone);

    /// <summary>🔹 3. الوقت كنص</summary>
    public string GetTimeString() =>
        Now.ToString("HH:mm:ss");

    /// <summary>🔹 4. التاريخ كنص</summary>
    public string GetDateString() =>
        Now.ToString("yyyy-MM-dd");

    /// <summary>🔹 5. التاريخ والوقت كنص</summary>
    public string GetDateTimeString() =>
        Now.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>🔹 6. أول يوم في الشهر الحالي</summary>
    public System.DateTime GetCurrentMonthStart()
    {
        var now = Now;
        return new System.DateTime(now.Year, now.Month, 1);
    }

    /// <summary>🔹 7. أول يوم في الشهر اللي فات</summary>
    public System.DateTime GetLastMonthStart() =>
        GetCurrentMonthStart().AddMonths(-1);

    /// <summary>🔹 8. آخر لحظة في الشهر اللي فات</summary>
    public System.DateTime GetLastMonthEnd() =>
        GetCurrentMonthStart().AddTicks(-1);

    /// <summary>🔹 9. تجميع الـ 3 قيم في مرة واحدة</summary>
    public (System.DateTime current, System.DateTime lastStart, System.DateTime lastEnd) GetMonthRange() =>
        (GetCurrentMonthStart(), GetLastMonthStart(), GetLastMonthEnd());
}
