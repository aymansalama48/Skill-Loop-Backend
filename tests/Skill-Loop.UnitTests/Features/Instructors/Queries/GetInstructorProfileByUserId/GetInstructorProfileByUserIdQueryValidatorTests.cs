using FluentValidation.TestHelper;
using Skill_Loop.Application.Features.Instructors.Queries.GetInstructorProfileByUserId;
using System;

namespace Skill_Loop.UnitTests.Features.Instructors.Queries.GetInstructorProfileByUserId;

public class GetInstructorProfileByUserIdQueryValidatorTests
{
    private readonly GetInstructorProfileByUserIdQueryValidator _validator;

    public GetInstructorProfileByUserIdQueryValidatorTests()
    {
        _validator = new GetInstructorProfileByUserIdQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_PassesValidation()
    {
        var query = new GetInstructorProfileByUserIdQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_HasValidationError()
    {
        var query = new GetInstructorProfileByUserIdQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("معرّف المستخدم مطلوب.");
    }
}
