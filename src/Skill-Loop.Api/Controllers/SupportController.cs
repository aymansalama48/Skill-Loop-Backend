using Skill_Loop.Application.Common.Errors.SupportQuestion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Skill_Loop.Api.Contracts.Support;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Api.Extensions;
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
/// إدارة طلبات الدعم الفني من المستخدمين
/// </summary>
[Route("api/support")]
[ApiController]
[Authorize]
public class SupportController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    public SupportController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }
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
    [HttpGet("questions/{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetSupportQuestionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupportQuestionByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("questions/{id:guid}/email-answer")]
    public async Task<IResult> SendFaqAnswerByEmail(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendFaqAnswerEmailCommand(id), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("contact")]
    // Throttled per IP: the contact form sends an email per submission, so an authenticated
    // user could otherwise flood the support inbox and the SMTP provider's quota.
    [EnableRateLimiting(RateLimitingExtensions.ContactFormPolicy)]
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
    [HttpGet("questions/mine")]
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
