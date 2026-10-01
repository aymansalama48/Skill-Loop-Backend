using FluentAssertions;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.UnitTests.Features.Categories;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateCategory()
    {
        var result = Category.Create("Programming", "icon.png", "Learn to code", 1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.Name.Should().Be("Programming");
        result.Data.IconUrl.Should().Be("icon.png");
        result.Data.Description.Should().Be("Learn to code");
        result.Data.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void Create_WithDefaultParameters_ShouldCreateCategoryWithDefaults()
    {
        var result = Category.Create("Math");

        result.IsSuccess.Should().BeTrue();
        result.Data!.IconUrl.Should().BeNull();
        result.Data.Description.Should().BeNull();
        result.Data.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void Create_GeneratesNewId()
    {
        var result1 = Category.Create("A");
        var result2 = Category.Create("B");

        result1.Data!.Id.Should().NotBe(result2.Data!.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_ShouldReturnValidationError(string name)
    {
        var result = Category.Create(name);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.EmptyName" && e.Type == ErrorType.Validation);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var result = Category.Create("  Programming  ");

        result.Data!.Name.Should().Be("Programming");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAllProperties()
    {
        var category = Category.Create("Programming", "old-icon.png", "Old desc", 1).Data!;

        var result = category.Update("Advanced Programming", "new-icon.png", "New desc", 5);

        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Advanced Programming");
        category.IconUrl.Should().Be("new-icon.png");
        category.Description.Should().Be("New desc");
        category.DisplayOrder.Should().Be(5);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithEmptyName_ShouldReturnValidationError(string name)
    {
        var category = Category.Create("Programming").Data!;

        var result = category.Update(name, null, null, 0);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.EmptyName");
    }

    [Fact]
    public void Update_TrimsName()
    {
        var category = Category.Create("Programming").Data!;

        category.Update("  Programming  ", null, null, 0);

        category.Name.Should().Be("Programming");
    }

    [Fact]
    public void Update_CanClearIconUrlByPassingNull()
    {
        var category = Category.Create("Programming", "icon.png", null, 0).Data!;

        category.Update("Programming", null, null, 0);

        category.IconUrl.Should().BeNull();
    }

    [Fact]
    public void Courses_ShouldBeEmptyOnCreation()
    {
        var category = Category.Create("Programming").Data!;

        category.Courses.Should().BeEmpty();
    }
}
