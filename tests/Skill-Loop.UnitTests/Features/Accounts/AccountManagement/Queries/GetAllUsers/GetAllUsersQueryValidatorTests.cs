namespace Skill_Loop.UnitTests.Features.Accounts.AccountManagement.Queries.GetAllUsers;

using FluentAssertions;
using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetAllUsers;
using Xunit;

public class GetAllUsersQueryValidatorTests
{
    private readonly GetAllUsersQueryValidator _validator;

    public GetAllUsersQueryValidatorTests()
    {
        _validator = new GetAllUsersQueryValidator();
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(5, 50)]
    [InlineData(1, 100)]
    public void Validate_ValidParameters_PassesValidation(int pageNumber, int pageSize)
    {
        var query = new GetAllUsersQuery(pageNumber, pageSize, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidPageNumber_HasValidationError(int invalidPageNumber)
    {
        var query = new GetAllUsersQuery(invalidPageNumber, 10, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage("رقم الصفحة يجب أن يكون أكبر من الصفر.");
    }

    [Theory]
    [InlineData(0, "حجم الصفحة يجب أن يكون أكبر من الصفر.")]
    [InlineData(-5, "حجم الصفحة يجب أن يكون أكبر من الصفر.")]
    [InlineData(101, "الحد الأقصى لحجم الصفحة هو 100.")]
    public void Validate_InvalidPageSize_HasValidationError(int invalidPageSize, string expectedErrorMessage)
    {
        var query = new GetAllUsersQuery(1, invalidPageSize, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(expectedErrorMessage);
    }
}
