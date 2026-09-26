namespace Skill_Loop.UnitTests.Features.Sessions.Commands.UpdateSession;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;
using System;
using Xunit;

public class UpdateSessionCommandValidatorTests
{
    private readonly UpdateSessionCommandValidator _validator;

    public UpdateSessionCommandValidatorTests()
    {
        _validator = new UpdateSessionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateSessionCommand(Guid.NewGuid(), "Valid Title");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var command = new UpdateSessionCommand(Guid.Empty, "Valid Title");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyTitle_HasValidationError(string? title)
    {
        var command = new UpdateSessionCommand(Guid.NewGuid(), title!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("عنوان الجلسة مطلوب.");
    }

    [Fact]
    public void Validate_TitleExceeds200Characters_HasValidationError()
    {
        var longTitle = new string('A', 201);
        var command = new UpdateSessionCommand(Guid.NewGuid(), longTitle);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("عنوان الجلسة لا يجب أن يتجاوز 200 حرف.");
    }
}
