using Skill_Loop.Application.Common.Errors.Auth;
using MediatR;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Behaviors;

/// <summary>
/// سلوك التحقق من الصلاحيات (Authorization Behavior).
/// يفحص وجود [PermissionAttribute] للطلب ويتحقق من هوية وصلاحيات المستخدم الحالي.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        var allowAnonymous = requestType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();
        if (allowAnonymous)
            return await next();

        var permissionAttribute = requestType.GetCustomAttributes(typeof(PermissionAttribute), true).FirstOrDefault() as PermissionAttribute;
        var authenticatedOnly = requestType.GetCustomAttributes(typeof(AuthenticatedOnlyAttribute), true).Any();

        if (permissionAttribute is null && !authenticatedOnly)
        {
            throw new InvalidOperationException($"Security marker missing for {requestType.Name}. You must specify [Permission(...)], [AuthenticatedOnly], or [AllowAnonymous].");
        }

        if (!currentUser.IsAuthenticated)
        {
            return ResultFactory.CreateFailure<TResponse>(AuthErrors.Unauthorized);
        }

        var userId = currentUser.UserId?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return ResultFactory.CreateFailure<TResponse>(AuthErrors.MissingIdentifier);
        }

        if (permissionAttribute is not null)
        {
            var hasPermission = currentUser.HasPermission(permissionAttribute.Name);
            if (!hasPermission)
            {
                return ResultFactory.CreateFailure<TResponse>(AuthErrors.Forbidden);
            }
        }

        return await next();
    }
}