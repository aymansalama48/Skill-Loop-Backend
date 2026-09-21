namespace Skill_Loop.Application.Common.Abstractions.Core;

public interface IDateTime
{
    DateTime Now { get; }
    string GetTimeString();
    string GetDateString();
    string GetDateTimeString();
    DateTime GetCurrentMonthStart();
    DateTime GetLastMonthStart();
    DateTime GetLastMonthEnd();
    (DateTime current, DateTime lastStart, DateTime lastEnd) GetMonthRange();
}