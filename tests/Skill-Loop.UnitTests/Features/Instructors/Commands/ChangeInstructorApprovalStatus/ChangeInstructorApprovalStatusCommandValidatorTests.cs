using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.ChangeInstructorApprovalStatus;

public class ChangeInstructorApprovalStatusCommandValidatorTests
{
    private readonly ChangeInstructorApprovalStatusCommandValidator _validator;

    public ChangeInstructorApprovalStatusCommandValidatorTests()
    {
        _validator = new ChangeInstructorApprovalStatusCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new ChangeInstructorApprovalStatusCommand(Guid.NewGuid(), IsApproved: true);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_HasValidationError()
    {
        var command = new ChangeInstructorApprovalStatusCommand(Guid.Empty, IsApproved: true);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            ;
    }
}

