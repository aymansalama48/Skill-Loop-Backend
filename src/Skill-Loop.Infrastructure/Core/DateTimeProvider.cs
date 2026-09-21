using Skill_Loop.Application.Common.Abstractions.Core;

namespace Skill_Loop.Infrastructure.Core;

public class DateTimeProvider : IDateTime
{
    private static readonly TimeZoneInfo EgyptTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

    // 🔹 1. الوقت الحالي
    public System.DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(System.DateTime.UtcNow, EgyptTimeZone);

    // 🔹 2. الوقت كنص
    public string GetTimeString() =>
        Now.ToString("HH:mm:ss");

    // 🔹 3. التاريخ كنص
    public string GetDateString() =>
        Now.ToString("yyyy-MM-dd");

    // 🔹 4. التاريخ والوقت كنص
    public string GetDateTimeString() =>
        Now.ToString("yyyy-MM-dd HH:mm:ss");

    // 🔹 5. أول يوم في الشهر الحالي
    public System.DateTime GetCurrentMonthStart()
    {
        var now = Now;
        return new System.DateTime(now.Year, now.Month, 1);
    }

    // 🔹 6. أول يوم في الشهر اللي فات
    public System.DateTime GetLastMonthStart() =>
        GetCurrentMonthStart().AddMonths(-1);

    // 🔹 7. آخر لحظة في الشهر اللي فات
    public System.DateTime GetLastMonthEnd() =>
        GetCurrentMonthStart().AddTicks(-1);

    // 🔹 8. تجميع الـ 3 قيم في مرة واحدة
    public (System.DateTime current, System.DateTime lastStart, System.DateTime lastEnd) GetMonthRange() =>
        (GetCurrentMonthStart(), GetLastMonthStart(), GetLastMonthEnd());
}