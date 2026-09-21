using Hangfire;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using System.Linq.Expressions;

namespace Skill_Loop.Infrastructure.External.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    public void Enqueue(Expression<Action> methodCall)
        => BackgroundJob.Enqueue(methodCall);

    public void Enqueue<T>(Expression<Action<T>> methodCall)
        => BackgroundJob.Enqueue(methodCall);

    // Hangfire نفسها بتدعم Task overloads جاهزة أصلًا وبتنتظرها صح جوّاها
    public void Enqueue(Expression<Func<Task>> methodCall)
        => BackgroundJob.Enqueue(methodCall);

    public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
        => BackgroundJob.Enqueue(methodCall);
}