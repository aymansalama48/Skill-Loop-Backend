using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.RemoveInstructorReview;

public class RemoveInstructorReviewCommandValidatorTests
{
    private readonly RemoveInstructorReviewCommandValidator _validator;

    public RemoveInstructorReviewCommandValidatorTests()
    {
        _validator = new RemoveInstructorReviewCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new RemoveInstructorReviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyProfileId_HasValidationError()
    {
        var command = new RemoveInstructorReviewCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InstructorProfileId)
            ;
    }

    [Fact]
    public void Validate_EmptyReviewId_HasValidationError()
    {
        var command = new RemoveInstructorReviewCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ReviewId)
            ;
    }
}

