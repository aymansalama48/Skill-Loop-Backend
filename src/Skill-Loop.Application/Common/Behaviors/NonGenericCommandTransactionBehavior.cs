using MediatR;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Transaction;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Behaviors;

/// <summary>
/// Transaction behavior for commands that do not return a payload
/// (<see cref="ICommand"/> rather than <see cref="ICommand{TResponse}"/>).
///
/// Why this exists: the existing <see cref="TransactionBehavior{TRequest,TResponse}"/> is
/// constrained to <c>where TRequest : ICommand&lt;TResponse&gt;</c>, so it never matches a
/// non-generic command. Commands such as DeactivatePromoCode, AssignPermissionToRole,
/// DeleteSupportQuestion and AddCourseReview therefore ran with no transaction at all:
/// if the handler wrote several entities and the third <c>SaveChangesAsync</c> threw, the
/// first two writes were already committed. That is a partial-write bug rather than a
/// security one, but on a permission change it means a role can end up with a subset of
/// the permissions that were requested.
///
/// Registering this alongside the generic behavior gives both command shapes identical
/// transactional semantics.
///
/// TResponse stays open (arity 2, matching <see cref="IPipelineBehavior{TRequest,TResponse}"/>)
/// because MediatR's AddOpenBehavior requires the implementation to have the same generic
/// arity as the service it implements. Constraining TRequest to the non-generic
/// <see cref="ICommand"/> is what keeps this behavior from overlapping with
/// <see cref="TransactionBehavior{TRequest,TResponse}"/>, which is constrained to
/// <c>ICommand&lt;TResponse&gt;</c> instead.
/// </summary>
public sealed class NonGenericCommandTransactionBehavior<TRequest, TResponse>(
    ITransactionManager transactionManager)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return await transactionManager.ExecuteAsync(async ct => await next(), cancellationToken);
    }
}
