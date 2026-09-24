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

[Route("api/v1/[controller]")]
public class CategoriesController : BaseApiController
{
    /// <summary>
    /// استرجاع كل التصنيفات (محفوظة بالكاش)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetCategories(CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إنشاء تصنيف جديد مع إمكانية رفع صورة (multipart/form-data)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    [Consumes("multipart/form-data")]                       // ✅ 1
    public async Task<IResult> CreateCategory(
        [FromForm] CreateCategoryRequest request,           // ✅ 2
        CancellationToken cancellationToken)
    {
        // ✅ 3: Map Request → Command، مع فتح Stream للصورة
        Stream? iconStream = null;
        if (request.IconFile is not null && request.IconFile.Length > 0)
        {
            iconStream = request.IconFile.OpenReadStream();
        }

        var command = new CreateCategoryCommand(
            request.Name,
            request.Slug,
            iconStream,
            request.IconFile?.FileName,
            request.Description,
            request.DisplayOrder);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تعديل تصنيف مع إمكانية تغيير الصورة (multipart/form-data)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    [Consumes("multipart/form-data")]                       // ✅
    public async Task<IResult> UpdateCategory(
        Guid id,
        [FromForm] UpdateCategoryRequest request,           // ✅
        CancellationToken cancellationToken)
    {
        Stream? iconStream = null;
        if (request.IconFile is not null && request.IconFile.Length > 0)
        {
            iconStream = request.IconFile.OpenReadStream();
        }

        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Slug,
            iconStream,
            request.IconFile?.FileName,
            request.Description,
            request.DisplayOrder);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف تصنيف
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public async Task<IResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}