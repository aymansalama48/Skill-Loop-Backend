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
}
