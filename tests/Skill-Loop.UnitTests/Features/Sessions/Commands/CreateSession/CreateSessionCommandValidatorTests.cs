namespace Skill_Loop.UnitTests.Features.Sessions.Commands.CreateSession;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Sessions.Commands.CreateSession;
using System;
using Xunit;

public class CreateSessionCommandValidatorTests
{
    private readonly CreateSessionCommandValidator _validator;

    public CreateSessionCommandValidatorTests()
    {
        _validator = new CreateSessionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateSessionCommand("Clean Architecture Session", Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyTitle_HasValidationError(string? title)
    {
        var command = new CreateSessionCommand(title!, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            ;
    }

    [Fact]
    public void Validate_TitleExceeds200Characters_HasValidationError()
    {
        var longTitle = new string('A', 201);
        var command = new CreateSessionCommand(longTitle, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            ;
    }

    [Fact]
    public void Validate_EmptyInstructorId_HasValidationError()
    {
        var command = new CreateSessionCommand("Valid Title", Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InstructorId);
    }
}

