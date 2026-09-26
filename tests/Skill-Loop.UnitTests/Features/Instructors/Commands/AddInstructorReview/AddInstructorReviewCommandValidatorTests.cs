using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.AddInstructorReview;

public class AddInstructorReviewCommandValidatorTests
{
    private readonly AddInstructorReviewCommandValidator _validator;

    public AddInstructorReviewCommandValidatorTests()
    {
        _validator = new AddInstructorReviewCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new AddInstructorReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 4, "Good instructor");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyProfileId_HasValidationError()
    {
        var command = new AddInstructorReviewCommand(Guid.Empty, Guid.NewGuid(), 4, "Comment");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InstructorProfileId)
            .WithErrorMessage("معرّف المدرب مطلوب.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_RatingOutside1To5_HasValidationError(int rating)
    {
        var command = new AddInstructorReviewCommand(Guid.NewGuid(), Guid.NewGuid(), rating, "Comment");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Rating)
            .WithErrorMessage("التقييم يجب أن يكون بين 1 و 5 نجوم.");
    }

    [Fact]
    public void Validate_CommentExceeds1000Characters_HasValidationError()
    {
        var command = new AddInstructorReviewCommand(Guid.NewGuid(), Guid.NewGuid(), 4, new string('A', 1001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }
}
