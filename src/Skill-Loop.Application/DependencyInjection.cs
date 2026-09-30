using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Behaviors;
using System.Reflection;

namespace Skill_Loop.Application;

public static class DependencyInjection
{
    /// <summary>
    /// تسجيل كافة خدمات طبقة الـ Application (MediatR, FluentValidation, AutoMapper/Mapster)
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. تسجيل MediatR للتعامل مع الـ Commands والـ Queries والـ Event Handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // 1. Logging أولاً لتتبع بداية ونهاية كل Request
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

            // 2. Performance لمراقبة الوقت الكلي
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));

            // 3. Authorization للتحقق من الصلاحيات قبل أي شيء آخر
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CourseOwnershipBehavior<,>));
            cfg.AddOpenBehavior(typeof(SessionOwnershipBehavior<,>));

            // 4. Validation للتأكد من صحة المدخلات
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

            // 5. Caching (Read) قراءة الكاش للـ Queries المؤهلة فقط
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));

            // 6. Cache Invalidation مسح الكاش للـ Commands الناجحة فقط
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));

            // 7. Transaction أخيراً لإدارة المعاملة أثناء تنفيذ الـ Handler
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));

            // 7b. نفس المعاملة للأوامر غير المُرجِعة لقيمة (ICommand بدون TResponse).
            //     TransactionBehavior قيوده على ICommand<TResponse> فكان بيتجاهلها بالكامل.
            cfg.AddOpenBehavior(typeof(NonGenericCommandTransactionBehavior<,>));
        });

        // 2. فحص الـ Assembly وتنسيق كافة كلاسات الـ FluentValidation تلقائياً
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}