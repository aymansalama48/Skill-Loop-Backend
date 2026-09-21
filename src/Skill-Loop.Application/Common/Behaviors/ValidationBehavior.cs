using FluentValidation;
using MediatR;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Behaviors;


/// <summary>
/// سلوك التحقق من صحة مدخلات الطلب (Validation Behavior).
/// يجمع أخطاء FluentValidation ويرجعها كـ Result.Failure بدون Exception.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessages = string.Join(" | ", failures.Select(f => f.ErrorMessage));

            var error = new Error(
                "Validation.Failed",
                errorMessages,
                ErrorType.Validation);

            return ResultFactory.CreateFailure<TResponse>(error);
        }

        return await next();
    }
}