namespace Skill_Loop.UnitTests.Features.Bookings.Commands.ChangeBookingStatus;

using FluentAssertions;
using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;
using Skill_Loop.Domain.Enums;
using System;
using Xunit;

public class ChangeBookingStatusCommandValidatorTests
{
    private readonly ChangeBookingStatusCommandValidator _validator;

    public ChangeBookingStatusCommandValidatorTests()
    {
        _validator = new ChangeBookingStatusCommandValidator();
    }

    [Theory]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.InProgress)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.NoShow)]
    public void Validate_InstructorAllowedStatuses_PassesValidation(BookingStatus status)
    {
        var command = new ChangeBookingStatusCommand(Guid.NewGuid(), Guid.NewGuid(), status);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Rejected_WithoutReason_HasValidationError()
    {
        var command = new ChangeBookingStatusCommand(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Rejected);

        var result = _validator.TestValidate(command);

        // الـ RuleFor(x => x) بيرجع خطأ على الـ Command نفسه
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "سبب الرفض مطلوب.");
    }

    [Fact]
    public void Validate_Rejected_WithReason_PassesValidation()
    {
        var command = new ChangeBookingStatusCommand(
            Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Rejected, "I am not available");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_CancelledStatus_IsNotAllowedForInstructor()
    {
        var command = new ChangeBookingStatusCommand(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Cancelled);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewStatus);
    }

    [Fact]
    public void Validate_EmptyIds_HaveValidationErrors()
    {
        var command = new ChangeBookingStatusCommand(Guid.Empty, Guid.Empty, BookingStatus.Completed);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BookingId);
        result.ShouldHaveValidationErrorFor(x => x.InstructorUserId);
    }
}
