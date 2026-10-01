using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Categories;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Categories.Commands.CreateCategory;
using Skill_Loop.Application.Features.Categories.Commands.DeleteCategory;
using Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;
using Skill_Loop.Application.Features.Categories.Queries.GetCategories;
using Skill_Loop.Domain.Constants;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة التصنيفات الخاصة بالكورسات والمجالات
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class CategoriesController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetCategories(CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost]
    public async Task<IResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(
            request.Name,
            request.Description,
            request.DisplayOrder);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.DisplayOrder);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/icon")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadCategoryIcon([FromRoute] Guid id, [FromForm] Skill_Loop.Api.Contracts.Common.UploadFileRequest request, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("Icon file is required.");
        }

        await using var stream = file.OpenReadStream();
        var command = new Skill_Loop.Application.Features.Categories.Commands.UploadCategoryIcon.UploadCategoryIconCommand(
            id,
            stream,
            file.FileName);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
