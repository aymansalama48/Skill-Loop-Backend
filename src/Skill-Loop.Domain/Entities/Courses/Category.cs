using Skill_Loop.Domain.Common.Errors.Category;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Courses;

public sealed class Category : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? IconUrl { get; private set; }
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }

    private readonly List<Course> _courses = [];
    public IReadOnlyCollection<Course> Courses => _courses.AsReadOnly();

    private Category() { }

    public static Result<Category> Create(string name, string? iconUrl = null, string? description = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Category>.Failure(CategoryErrors.EmptyName);

        return Result<Category>.Success(new Category
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            IconUrl = iconUrl,
            Description = description,
            DisplayOrder = displayOrder
        });
 
    }
    public Result Update(string name, string? iconUrl, string? description, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CategoryErrors.EmptyName);

        Name = name.Trim();
        IconUrl = iconUrl;
        Description = description;
        DisplayOrder = displayOrder;

        return Result.Success();
    }

    public void UpdateIcon(string iconUrl)
    {
        IconUrl = iconUrl;
    }
}
