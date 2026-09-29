using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Commands.CreateMyInstructorProfile;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.CreateMyInstructorProfile;

public class CreateMyInstructorProfileCommandValidatorTests
{
    private readonly CreateMyInstructorProfileCommandValidator _validator;

    public CreateMyInstructorProfileCommandValidatorTests()
    {
        _validator = new CreateMyInstructorProfileCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateMyInstructorProfileCommand(Guid.NewGuid(), "Senior Developer", "A great bio.");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyHeadline_HasValidationError(string headline)
    {
        var command = new CreateMyInstructorProfileCommand(Guid.NewGuid(), headline, "A bio.");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Headline)
            .WithErrorMessage("العنوان التعريفي مطلوب.");
    }

    [Fact]
    public void Validate_EmptyBio_HasValidationError()
    {
        var command = new CreateMyInstructorProfileCommand(Guid.NewGuid(), "Headline", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Bio)
            .WithErrorMessage("النبذة التعريفية مطلوبة.");
    }

    [Fact]
    public void Validate_HeadlineExceeds200Characters_HasValidationError()
    {
        var longHeadline = new string('A', 201);
        var command = new CreateMyInstructorProfileCommand(Guid.NewGuid(), longHeadline, "Bio.");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Headline);
    }

    [Fact]
    public void Validate_BioExceeds2000Characters_HasValidationError()
    {
        var longBio = new string('A', 2001);
        var command = new CreateMyInstructorProfileCommand(Guid.NewGuid(), "Headline", longBio);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Bio);
    }
}
