using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;
using System;

namespace Skill_Loop.UnitTests.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator;

    public UpdateCategoryCommandValidatorTests()
    {
        _validator = new UpdateCategoryCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Programming", "programming", null, null, "Desc", 1);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var command = new UpdateCategoryCommand(Guid.Empty, "Programming", "programming", null, null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyName_HasValidationError(string name)
    {
        var command = new UpdateCategoryCommand(Guid.NewGuid(), name, "programming", null, null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptySlug_HasValidationError(string slug)
    {
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Programming", slug, null, null, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }
}
