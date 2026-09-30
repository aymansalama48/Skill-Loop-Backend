using Skill_Loop.Application.Common.Errors.Session;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Features.Sessions.Common;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Common.Behaviors;

public sealed class SessionOwnershipBehavior<TRequest, TResponse>(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ISessionCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (currentUser.HasPermission(Permissions.Sessions.ManageAll))
        {
            return await next();
        }

        var session = await dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            var error = SessionErrors.NotFound;
            return ResultFactory.CreateFailure<TResponse>(error);
        }

        if (session.InstructorId != currentUser.UserId)
        {
            var error = SessionErrors.Forbidden;
            return ResultFactory.CreateFailure<TResponse>(error);
        }

        return await next();
    }
}
