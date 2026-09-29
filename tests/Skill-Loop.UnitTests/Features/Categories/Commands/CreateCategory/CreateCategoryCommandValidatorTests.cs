using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Categories.Commands.CreateCategory;
using System;
using System.IO;

namespace Skill_Loop.UnitTests.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator;

    public CreateCategoryCommandValidatorTests()
    {
        _validator = new CreateCategoryCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateCategoryCommand("Programming", "programming", null, null, "Desc", 1);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyName_HasValidationError(string name)
    {
        var command = new CreateCategoryCommand(name, "programming", null, null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptySlug_HasValidationError(string slug)
    {
        var command = new CreateCategoryCommand("Programming", slug, null, null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_NameExceeds100Characters_HasValidationError()
    {
        var command = new CreateCategoryCommand(new string('A', 101), "programming", null, null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_SlugExceeds150Characters_HasValidationError()
    {
        var command = new CreateCategoryCommand("Programming", new string('A', 151), null, null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_NegativeDisplayOrder_HasValidationError(int displayOrder)
    {
        var command = new CreateCategoryCommand("Programming", "programming", null, null, null, displayOrder);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DisplayOrder);
    }
}
