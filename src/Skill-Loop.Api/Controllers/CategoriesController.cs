using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Categories.Commands.CreateCategory;
using Skill_Loop.Application.Features.Categories.Commands.DeleteCategory;
using Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;
using Skill_Loop.Application.Features.Categories.Queries.GetCategories;
using Skill_Loop.Domain.Constants;

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

    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public async Task<IResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public async Task<IResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(new { message = "معرف التصنيف في المسار لا يتطابق مع المعرف في البيانات." });
        }

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public async Task<IResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
