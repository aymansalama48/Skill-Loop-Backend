namespace Skill_Loop.Api.Contracts.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder = 0);