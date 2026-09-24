namespace Skill_Loop.Api.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string Slug,
    IFormFile? IconFile,
    string? Description,
    int DisplayOrder = 0);