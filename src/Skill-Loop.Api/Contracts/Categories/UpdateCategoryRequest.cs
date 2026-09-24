namespace Skill_Loop.Api.Contracts.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string Slug,
    IFormFile? IconFile,
    string? Description,
    int DisplayOrder = 0);