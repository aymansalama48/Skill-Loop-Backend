namespace Skill_Loop.UnitTests.Features.Sessions.Queries.GetSessionsPaged;

using FluentValidation.TestHelper;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;
using Xunit;

public class GetSessionsPagedQueryValidatorTests
{
    private readonly GetSessionsPagedQueryValidator _validator;

    public GetSessionsPagedQueryValidatorTests()
    {
        _validator = new GetSessionsPagedQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_PassesValidation()
    {
        var query = new GetSessionsPagedQuery(new PaginationParameters { PageNumber = 1, PageSize = 10 });

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullPagination_HasValidationError()
    {
        var query = new GetSessionsPagedQuery(null!);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination)
            ;
    }
}

