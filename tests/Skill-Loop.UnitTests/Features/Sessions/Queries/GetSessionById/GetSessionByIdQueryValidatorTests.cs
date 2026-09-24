namespace Skill_Loop.UnitTests.Features.Sessions.Queries.GetSessionById;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionById;
using System;
using Xunit;

public class GetSessionByIdQueryValidatorTests
{
    private readonly GetSessionByIdQueryValidator _validator;

    public GetSessionByIdQueryValidatorTests()
    {
        _validator = new GetSessionByIdQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_PassesValidation()
    {
        var query = new GetSessionByIdQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_HasValidationError()
    {
        var query = new GetSessionByIdQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
