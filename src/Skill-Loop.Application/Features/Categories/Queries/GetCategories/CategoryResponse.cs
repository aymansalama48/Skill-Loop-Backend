namespace Skill_Loop.Application.Features.Categories.Queries.GetCategories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl,
    string? Description,
    int DisplayOrder,
    int PublishedCoursesCount);