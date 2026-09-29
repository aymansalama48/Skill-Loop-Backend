using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Queries.GetInstructorsPaged;

public class GetInstructorsPagedQueryValidatorTests
{
    private readonly GetInstructorsPagedQueryValidator _validator;

    public GetInstructorsPagedQueryValidatorTests()
    {
        _validator = new GetInstructorsPagedQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_PassesValidation()
    {
        var query = new GetInstructorsPagedQuery(1, 10, "react", 4.0, true, "rating");

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageNumberNotGreaterThanZero_HasValidationError(int pageNumber)
    {
        var query = new GetInstructorsPagedQuery(pageNumber, 10, null, null, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage("رقم الصفحة يجب أن يكون أكبر من الصفر.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_PageSizeNotGreaterThanZero_HasValidationError(int pageSize)
    {
        var query = new GetInstructorsPagedQuery(1, pageSize, null, null, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_PageSizeExceeds100_HasValidationError()
    {
        var query = new GetInstructorsPagedQuery(1, 101, null, null, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("الحد الأقصى لحجم الصفحة هو 100.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void Validate_MinRatingOutsideRange_HasValidationError(double rating)
    {
        var query = new GetInstructorsPagedQuery(1, 10, null, rating, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.MinRating)
            .WithErrorMessage("التقييم يجب أن يكون بين 0 و 5 نجوم.");
    }
}
