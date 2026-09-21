using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Common.Abstractions.External.Cache;

// 1. الواجهة الأساسية (بدون TResponse) عشان الـ Behavior يقدر يشوفها
public interface ICacheableQuery
{
    string CacheKey { get; }

    TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);

    TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}

// 2. الواجهة اللي هنستخدمها في الأكواد بتاعتنا (بتورث من الـ IQuery والواجهة الأساسية)
public interface ICacheableQuery<TResponse> : IQuery<TResponse>, ICacheableQuery
{
}