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
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Programming", "Desc", 1);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var command = new UpdateCategoryCommand(Guid.Empty, "Programming", null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyName_HasValidationError(string name)
    {
        var command = new UpdateCategoryCommand(Guid.NewGuid(), name, null, 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

}
