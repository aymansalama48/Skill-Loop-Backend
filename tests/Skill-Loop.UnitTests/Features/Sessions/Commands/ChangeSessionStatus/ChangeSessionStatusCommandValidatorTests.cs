namespace Skill_Loop.UnitTests.Features.Sessions.Commands.ChangeSessionStatus;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;
using Skill_Loop.Domain.Enums;
using System;
using Xunit;

public class ChangeSessionStatusCommandValidatorTests
{
    private readonly ChangeSessionStatusCommandValidator _validator;

    public ChangeSessionStatusCommandValidatorTests()
    {
        _validator = new ChangeSessionStatusCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new ChangeSessionStatusCommand(Guid.NewGuid(), SessionStatus.Published);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var command = new ChangeSessionStatusCommand(Guid.Empty, SessionStatus.Published);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_InvalidStatus_HasValidationError()
    {
        var command = new ChangeSessionStatusCommand(Guid.NewGuid(), (SessionStatus)999);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewStatus)
            ;
    }
}

