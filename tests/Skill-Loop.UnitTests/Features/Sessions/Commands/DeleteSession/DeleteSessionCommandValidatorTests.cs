namespace Skill_Loop.UnitTests.Features.Sessions.Commands.DeleteSession;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Sessions.Commands.DeleteSession;
using System;
using Xunit;

public class DeleteSessionCommandValidatorTests
{
    private readonly DeleteSessionCommandValidator _validator;

    public DeleteSessionCommandValidatorTests()
    {
        _validator = new DeleteSessionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new DeleteSessionCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var command = new DeleteSessionCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
