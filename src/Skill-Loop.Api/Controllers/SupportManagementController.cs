using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Support;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;
using Skill_Loop.Application.Features.Support.Commands.CreateSupportQuestion;
using Skill_Loop.Application.Features.Support.Commands.DeleteSupportQuestion;
using Skill_Loop.Application.Features.Support.Commands.SetSupportQuestionPublication;
using Skill_Loop.Application.Features.Support.Commands.UpdateSupportQuestion;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionDetails;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionsPaged;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة التذاكر وطلبات الدعم من قِبل الإدارة
/// </summary>
[Route("api/support/manage")]
[ApiController]
[Authorize(Roles = "Admin,Staff")]
public class SupportManagementController : BaseApiController
{
    private readonly IMediator _mediator;
    public SupportManagementController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet("questions")]
    public async Task<IResult> GetSupportQuestions(
        [FromQuery] GetSupportQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetSupportQuestionsPagedQuery(
            new PaginationParameters { PageNumber = request.PageNumber, PageSize = request.PageSize },
            request.SearchTerm,
            request.Category,
            request.IsPublished,
            request.IsAnswered);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("questions/unanswered")]
    public async Task<IResult> GetUnansweredSupportQuestions(
        [FromQuery] GetSupportQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetSupportQuestionsPagedQuery(
            new PaginationParameters { PageNumber = request.PageNumber, PageSize = request.PageSize },
            request.SearchTerm,
            request.Category,
            request.IsPublished,
            IsAnswered: false);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("questions/{id:guid}")]
    public async Task<IResult> GetSupportQuestionDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupportQuestionDetailsQuery(id), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("questions")]
    public async Task<IResult> CreateSupportQuestion(
        [FromBody] CreateSupportQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSupportQuestionCommand(
            request.Question,
            request.Answer,
            request.Category,
            request.IsPublished);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("questions/{id:guid}/answer")]
    public async Task<IResult> AnswerSupportQuestion(
        Guid id,
        [FromBody] AnswerSupportQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AnswerSupportQuestionCommand(id, request.Answer, request.Publish);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("questions/{id:guid}")]
    public async Task<IResult> UpdateSupportQuestion(
        Guid id,
        [FromBody] UpdateSupportQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSupportQuestionCommand(
            id,
            request.Question,
            request.Answer,
            request.Category,
            request.IsPublished);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPatch("questions/{id:guid}/publication")]
    public async Task<IResult> SetSupportQuestionPublication(
        Guid id,
        [FromBody] SetSupportQuestionPublicationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetSupportQuestionPublicationCommand(id, request.IsPublished);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpDelete("questions/{id:guid}")]
    public async Task<IResult> DeleteSupportQuestion(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSupportQuestionCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
