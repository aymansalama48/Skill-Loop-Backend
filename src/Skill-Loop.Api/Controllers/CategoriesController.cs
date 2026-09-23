using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Courses.Queries.GetCategories;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/[controller]")]
public class CategoriesController : BaseApiController
{
    /// <summary>
    /// استرجاع كل التصنيفات الخاصة بالكورسات والمحفوظة بالكاش
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetCategories(CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
