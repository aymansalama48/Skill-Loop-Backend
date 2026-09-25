namespace Skill_Loop.Api.Contracts.Instructors;

public sealed record GetInstructorsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    double? MinRating = null,
    bool? HasCompletedSessions = null,
    string? SortBy = null
);