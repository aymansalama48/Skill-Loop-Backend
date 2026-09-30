namespace Skill_Loop.Application.Common.Abstractions.Core;

public interface IDateTime
{
    /// <summary>
    /// Current time in <see cref="DateTimeKind.Utc"/>.
    ///
    /// Use this for anything persisted or protocol-facing: token expiry, audit columns
    /// suffixed "Utc", idempotency windows, and background-job scheduling. The former
    /// <c>Now</c> returned Egypt local time, which silently shifted every stored timestamp
    /// and made JWT expiry drift by the UTC offset (and by DST on top of that).
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Current time in the application's display timezone. Use only for formatting
    /// timestamps for display — never for storage or comparison.
    /// </summary>
    DateTime Now { get; }

    string GetTimeString();
    string GetDateString();
    string GetDateTimeString();
    DateTime GetCurrentMonthStart();
    DateTime GetLastMonthStart();
    DateTime GetLastMonthEnd();
    (DateTime current, DateTime lastStart, DateTime lastEnd) GetMonthRange();
}