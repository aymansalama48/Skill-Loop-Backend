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
        var permissionAttribute = request.GetType()
            .GetCustomAttributes(typeof(PermissionAttribute), true)
            .FirstOrDefault() as PermissionAttribute;

        if (permissionAttribute is null)
            return await next();

        if (!currentUser.IsAuthenticated)
        {
            var error = AuthErrors.Unauthorized;
            return ResultFactory.CreateFailure<TResponse>(error);
        }

        var userId = currentUser.UserId.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            var error = AuthErrors.MissingIdentifier;
            return ResultFactory.CreateFailure<TResponse>(error);
        }

        var hasPermission = currentUser.HasPermission(permissionAttribute.Name);
        if (!hasPermission)
        {
            var error = AuthErrors.Forbidden;
            return ResultFactory.CreateFailure<TResponse>(error);
        }

        return await next();
    }
}