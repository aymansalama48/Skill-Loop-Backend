namespace Skill_Loop.UnitTests.Features.Sessions.Commands.UpdateSession;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;
using Skill_Loop.Domain.Enums;
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
        // تمرير جميع المعاملات المطلوبة (Id, Title, Price, Duration, Type)
        var command = new UpdateSessionCommand(Guid.NewGuid(), "Valid Title", 100, 60, SessionType.Online);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var command = new UpdateSessionCommand(Guid.Empty, "Valid Title", 100, 60, SessionType.Online);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyTitle_HasValidationError(string? title)
    {
        var command = new UpdateSessionCommand(Guid.NewGuid(), title!, 100, 60, SessionType.Online);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("عنوان الجلسة مطلوب.");
    }

    [Fact]
    public void Validate_TitleExceeds200Characters_HasValidationError()
    {
        var longTitle = new string('A', 201);
        var command = new UpdateSessionCommand(Guid.NewGuid(), longTitle, 100, 60, SessionType.Online);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("عنوان الجلسة لا يجب أن يتجاوز 200 حرف.");
    }
}