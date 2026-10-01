namespace Skill_Loop.Api.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder = 0);