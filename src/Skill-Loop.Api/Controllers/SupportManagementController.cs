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
/// إدارة الاستفسارات لفريق الدعم والـ Admin: عرض الكل، الرد، النشر، التعديل والحذف.
/// كل الـ endpoints هنا محمية بـ <c>Admin,Staff</c> على مستوى الكنترولر.
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

    /// <summary>
    /// List every support question, including unanswered and unpublished ones.
    /// </summary>
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

    /// <summary>
    /// List questions still waiting for an answer.
    /// </summary>
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

    /// <summary>
    /// Get the full details of a support question, including the asker's email.
    /// </summary>
    [HttpGet("questions/{id:guid}")]
    public async Task<IResult> GetSupportQuestionDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupportQuestionDetailsQuery(id), cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Create a new support question (FAQ)
    /// </summary>
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

    /// <summary>
    /// Answer a support question and email the answer to the asker.
    /// </summary>
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

    /// <summary>
    /// Update a support question
    /// </summary>
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

    /// <summary>
    /// Publish or unpublish a support question
    /// </summary>
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

    /// <summary>
    /// Delete a support question
    /// </summary>
    [HttpDelete("questions/{id:guid}")]
    public async Task<IResult> DeleteSupportQuestion(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSupportQuestionCommand(id), cancellationToken);

        return HandleResult(result);
    }
}
