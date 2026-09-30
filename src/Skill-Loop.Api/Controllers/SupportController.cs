using Skill_Loop.Application.Common.Errors.SupportQuestion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Support;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;
using Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;
using Skill_Loop.Application.Features.Support.Queries.GetMySupportQuestions;
using Skill_Loop.Application.Features.Support.Queries.GetPublishedSupportQuestionsPaged;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Api.Controllers;

/// <summary>
/// واجهة الدعم الموجّهة للمستخدم: الـ FAQ العام (Anonymous) واستفسارات المستخدم المسجّل.
/// إدارة الاستفسارات (رد/نشر/حذف) في <see cref="SupportManagementController"/>.
/// </summary>
[Route("api/support")]
[ApiController]
public class SupportController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public SupportController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Search the public FAQ. Only published questions are returned.
    /// </summary>
    [HttpGet("questions")]
    [AllowAnonymous]
    public async Task<IResult> GetPublishedSupportQuestions(
        [FromQuery] GetPublishedSupportQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetPublishedSupportQuestionsPagedQuery(
            new PaginationParameters { PageNumber = request.PageNumber, PageSize = request.PageSize },
            request.SearchTerm,
            request.Category);

        var result = await _mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Get a published support question by ID.
    /// </summary>
    [HttpGet("questions/{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetSupportQuestionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupportQuestionByIdQuery(id), cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Send the published answer of a FAQ question to the logged-in user's email.
    /// </summary>
    [HttpPost("questions/{id:guid}/email-answer")]
    [Authorize]
    public async Task<IResult> SendFaqAnswerByEmail(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendFaqAnswerEmailCommand(id), cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Submit a support question. The asker's name and email come from the JWT, not the body.
    /// </summary>
    [HttpPost("contact")]
    [Authorize]
    public async Task<IResult> SubmitContactForm(
        [FromBody] SubmitContactFormRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitContactFormCommand(
            request.Subject,
            request.Message,
            request.Category);

        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// List the support questions submitted by the current user.
    /// </summary>
    [HttpGet("questions/mine")]
    [Authorize]
    public async Task<IResult> GetMySupportQuestions(
        [FromQuery] GetMySupportQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
        {
            return HandleResult(Result.Failure(
                SupportQuestionErrors.MissingIdentifier));
        }

        var query = new GetMySupportQuestionsQuery(
            userId.Value,
            new PaginationParameters { PageNumber = request.PageNumber, PageSize = request.PageSize });

        var result = await _mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}
