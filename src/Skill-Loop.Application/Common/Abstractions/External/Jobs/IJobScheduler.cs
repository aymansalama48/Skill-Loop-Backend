using System.Linq.Expressions;

namespace Skill_Loop.Application.Common.Abstractions.External.Jobs;

public interface IJobScheduler
{
    void Enqueue(Expression<Action> methodCall);
    void Enqueue<T>(Expression<Action<T>> methodCall);

    // 👈 الاتنين دول مضافين — للـ Methods اللي بترجع Task
    void Enqueue(Expression<Func<Task>> methodCall);
    void Enqueue<T>(Expression<Func<T, Task>> methodCall);
}