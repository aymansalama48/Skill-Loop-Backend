using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Queries.GetInstructorFullProfileByUserId;

public class GetInstructorFullProfileByUserIdQueryValidatorTests
{
    private readonly GetInstructorFullProfileByUserIdQueryValidator _validator;

    public GetInstructorFullProfileByUserIdQueryValidatorTests()
    {
        _validator = new GetInstructorFullProfileByUserIdQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_PassesValidation()
    {
        var query = new GetInstructorFullProfileByUserIdQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_HasValidationError()
    {
        var query = new GetInstructorFullProfileByUserIdQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            ;
    }
}

