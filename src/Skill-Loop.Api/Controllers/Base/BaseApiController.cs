using Skill_Loop.Api.Extensions;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Domain.Common.Results;


namespace Skill_Loop.Api.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// حقن تلقائي لـ MediatR Sender من حاوية DI
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// تحويل الـ Result إلى HTTP Response قياسي بنفس الـ HttpContext
    /// </summary>
    protected IResult HandleResult(Result result) => result.ToHttpResponse(HttpContext);

    protected IResult HandleResult<T>(Result<T> result) => result.ToHttpResponse(HttpContext);

    /// <summary>
    /// Returns the authenticated user's id, or throws a 401 if the claim is absent.
    ///
    /// Security: the previous pattern was <c>_currentUser.UserId ?? Guid.Empty</c>. That is
    /// fail-open — <c>Guid.Empty</c> is a real, non-null value, so it flows into the command
    /// and the handler runs its queries and writes its rows against a user that does not
    /// exist. The result is either silent data loss (reads return nothing, so a caller
    /// concludes "no data" instead of "not authenticated") or rows persisted under a shared
    /// sentinel identity that a different caller can then collide with.
    ///
    /// A missing user-id claim on an authorized endpoint is an authentication failure, not
    /// a data condition, so it is reported as 401 and the command is never constructed.
    /// </summary>
    protected Guid RequireUserId()
    {
        var userId = HttpContext.RequestServices
            .GetRequiredService<ICurrentUser>()
            .UserId;

        if (userId is not { } id || id == Guid.Empty)
            throw new UnauthorizedAccessException(
                "The authenticated request does not carry a valid user id claim.");

        return id;
    }
}