namespace Skill_Loop.UnitTests.Features.Accounts.AccountManagement.Queries.GetUserById;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetUserById;
using System;
using Xunit;

public class GetUserByIdQueryValidatorTests
{
    private readonly GetUserByIdQueryValidator _validator;

    public GetUserByIdQueryValidatorTests()
    {
        _validator = new GetUserByIdQueryValidator();
    }

    [Fact]
    public void Validate_ValidUserId_PassesValidation()
    {
        var query = new GetUserByIdQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_HasValidationError()
    {
        var query = new GetUserByIdQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("معرف المستخدم مطلوب وغير صالح.");
    }
}
