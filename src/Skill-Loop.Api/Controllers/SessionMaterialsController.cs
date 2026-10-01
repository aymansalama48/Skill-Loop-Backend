using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Sessions;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Materials.Commands.DeleteSessionMaterial;
using Skill_Loop.Application.Features.Sessions.Materials.Commands.ReorderSessionMaterials;
using Skill_Loop.Application.Features.Sessions.Materials.Commands.UploadSessionMaterial;
using Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterialDownloadInfo;
using Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterials;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة الملحقات والمواد الخاصة بالجلسات
/// </summary>
[Authorize]
[Route("api/v1/sessions")]
public sealed class SessionMaterialsController(
    ICourseContentStorage courseContentStorage) : BaseApiController
{
    [HttpPost("{sessionId:guid}/materials")]
    [Consumes("multipart/form-data")] // هذا السطر هو السحر الذي يفهمه Scalar
    public async Task<IResult> UploadSessionMaterial(
        [FromRoute] Guid sessionId,
        [FromForm] UploadSessionMaterialRequest request,
        CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var command = new UploadSessionMaterialCommand(
            sessionId,
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.MaterialType); // تمرير النوع لو تمت إضافته
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("{sessionId:guid}/materials")]
    public async Task<IResult> GetSessionMaterials(
        Guid sessionId,
        [FromQuery] GetSessionMaterialsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionMaterialsQuery(
            sessionId,
            new PaginationParameters { PageNumber = request.PageNumber, PageSize = request.PageSize });
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("{sessionId:guid}/materials/{materialId:guid}/download")]
    public async Task<IResult> DownloadSessionMaterial(
        Guid sessionId,
        Guid materialId,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionMaterialDownloadInfoQuery(sessionId, materialId);
        var queryResult = await Mediator.Send(query, cancellationToken);
        if (!queryResult.Succeeded)
        {
            return HandleResult(queryResult);
        }
        var downloadResult = await courseContentStorage.DownloadAsync(
            queryResult.Data.DriveFileId,
            cancellationToken);
        return HandleResult(downloadResult);
    }
    [HttpDelete("{sessionId:guid}/materials/{materialId:guid}")]
    public async Task<IResult> DeleteSessionMaterial(
        Guid sessionId,
        Guid materialId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new DeleteSessionMaterialCommand(sessionId, materialId),
            cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("{sessionId:guid}/materials/reorder")]
    public async Task<IResult> ReorderSessionMaterials(
        Guid sessionId,
        [FromBody] ReorderSessionMaterialsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ReorderSessionMaterialsCommand(
                sessionId,
                request.Materials.Select(m => m.MaterialId).ToList()),
            cancellationToken);
        return HandleResult(result);
    }
}
