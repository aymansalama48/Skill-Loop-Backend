using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateInstructorReview;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.UpdateInstructorReview;

public class UpdateInstructorReviewCommandValidatorTests
{
    private readonly UpdateInstructorReviewCommandValidator _validator;

    public UpdateInstructorReviewCommandValidatorTests()
    {
        _validator = new UpdateInstructorReviewCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateInstructorReviewCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, "Updated");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyProfileId_HasValidationError()
    {
        var command = new UpdateInstructorReviewCommand(
            Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 4, "Updated");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InstructorProfileId)
            ;
    }

    [Fact]
    public void Validate_EmptyReviewId_HasValidationError()
    {
        var command = new UpdateInstructorReviewCommand(
            Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 4, "Updated");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ReviewId)
            ;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_RatingOutside1To5_HasValidationError(int rating)
    {
        var command = new UpdateInstructorReviewCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), rating, "Updated");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Validate_CommentExceeds1000Characters_HasValidationError()
    {
        var command = new UpdateInstructorReviewCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, new string('A', 1001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }
}

