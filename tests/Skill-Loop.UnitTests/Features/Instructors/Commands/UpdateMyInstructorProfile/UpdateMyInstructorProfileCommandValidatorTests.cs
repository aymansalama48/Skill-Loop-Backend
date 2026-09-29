using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateMyInstructorProfile;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.UpdateMyInstructorProfile;

public class UpdateMyInstructorProfileCommandValidatorTests
{
    private readonly UpdateMyInstructorProfileCommandValidator _validator;

    public UpdateMyInstructorProfileCommandValidatorTests()
    {
        _validator = new UpdateMyInstructorProfileCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateMyInstructorProfileCommand(Guid.NewGuid(), "Headline", "Bio");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyHeadline_HasValidationError(string headline)
    {
        var command = new UpdateMyInstructorProfileCommand(Guid.NewGuid(), headline, "Bio");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Headline)
            .WithErrorMessage("العنوان التعريفي مطلوب.");
    }

    [Fact]
    public void Validate_EmptyBio_HasValidationError()
    {
        var command = new UpdateMyInstructorProfileCommand(Guid.NewGuid(), "Headline", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Bio)
            .WithErrorMessage("النبذة التعريفية مطلوبة.");
    }
}
