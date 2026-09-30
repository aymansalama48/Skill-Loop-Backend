using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Categories.Commands.DeleteCategory;
using System;

namespace Skill_Loop.UnitTests.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandValidatorTests
{
    private readonly DeleteCategoryCommandValidator _validator;

    public DeleteCategoryCommandValidatorTests()
    {
        _validator = new DeleteCategoryCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var command = new DeleteCategoryCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            ;
    }
}

