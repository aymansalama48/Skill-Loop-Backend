using Skill_Loop.Domain.Common.Results;
using System.Reflection;

namespace Skill_Loop.Application.Common.Helpers;

public static class ResultFactory
{
    public static TResponse CreateFailure<TResponse>(Error error)
    {
        // 1. لو الرد من نوع Result العادي (بدون Generic)
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        // 2. لو الرد من نوع Result<T> (زي PagedResult<DoctorResponse>)
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];

            // استخدام الـ Reflection بشكل آمن للبحث عن دالة Failure
            var failureMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethods(BindingFlags.Public | BindingFlags.Static) // 👈 السر هنا
                .FirstOrDefault(m => m.Name == nameof(Result.Failure) &&
                                     m.GetParameters().Length == 1 &&
                                     m.GetParameters()[0].ParameterType == typeof(Error));

            if (failureMethod is not null)
            {
                return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
            }
        }

        throw new InvalidOperationException($"Cannot create a failure result for type {typeof(TResponse).Name}. Make sure the Result<T> class has a static Failure(Error error) method.");
    }
}