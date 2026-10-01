using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Sessions;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Sessions.Commands.AddSessionReview;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة التقييمات والمراجعات الخاصة بالجلسات
/// </summary>
[Route("api/v1/sessions/{sessionId:guid}/reviews")]
[Authorize]
[Tags("Session Reviews")]
public class SessionReviewsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public SessionReviewsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    /// <summary>
    /// إضافة تقييم جديد للجلسة
    /// </summary>
    [HttpPost]
    public async Task<IResult> AddReview(
        Guid sessionId,
        [FromBody] AddSessionReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new AddSessionReviewCommand(userId, sessionId, request.Stars, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// عرض تقييمات الجلسة مع التصفح
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetReviews(
        Guid sessionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new Skill_Loop.Application.Features.Sessions.Queries.GetSessionReviews.GetSessionReviewsQuery(sessionId, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تعديل تقييم مسجل مسبقاً للجلسة
    /// </summary>
    [HttpPut]
    public async Task<IResult> UpdateReview(
        Guid sessionId,
        [FromBody] UpdateSessionReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new Skill_Loop.Application.Features.Sessions.Commands.UpdateSessionReview.UpdateSessionReviewCommand(sessionId, userId, request.Stars, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف تقييم الجلسة
    /// </summary>
    [HttpDelete]
    public async Task<IResult> DeleteReview(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var command = new Skill_Loop.Application.Features.Sessions.Commands.DeleteSessionReview.DeleteSessionReviewCommand(sessionId, userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
