using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Skill_Loop.Api.Middlewares;
using Skill_Loop.Infrastructure.DependencyInjection;
using Skill_Loop.Application;
namespace Skill_Loop.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل كافة خدمات التطبيق والطبقات الخارجية في حاوية DI
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // 1. تسجيل طبقات البنية التحتية والتطبيقات
        services.AddInfrastructure(configuration);
        services.AddApplication();

        // 2. إعداد الـ Controllers وتوحيد أخطاء الـ Model Binding مع ProblemDetails
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Error",
                        Detail = "توجد أخطاء في البيانات المدخلة في الـ Payload.",
                        Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
                    };

                    problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    return new BadRequestObjectResult(problemDetails);
                };
            });

        // 2.1 منع تسجيل أي Controller داخل نطاق ".Dev" في بيئات غير التطوير.
        //
        // الاعتماد على فحص `_env.IsDevelopment()` جوّه الـ Action جواهل: لو الـ Environment
        // اتظبط غلط (مثلاً ASPNETCORE_ENVIRONMENT ناقص في الحاوية) الـ Endpoint بيبقى
        // موجود في الـ Routing Table وبيظهر في الـ Swagger. الحذف من الـ Application Model
        // بيخلي الـ Endpoint مش موجود أصلاً، فمفيش حالة بتسمح بوجوده.
        if (!environment.IsDevelopment())
        {
            services.Configure<MvcOptions>(options =>
                options.Conventions.Add(new DevControllerRemovalConvention()));
        }

        // 2.5 الشات اللحظي (SignalR)
        services.AddChatRealtime();

        // 3. إضافة دعم الـ ProblemDetails والـ Global Exception Handler
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // 4. الصلاحيات والتوثيق المكتبي والـ CORS
        services.AddAuthorization();
        services.AddOpenApiDocumentation();
        services.AddCorsPolicy(configuration);

        // 5. حد المعدل (Rate Limiter) — لازم يتسجل قبل MapControllers
        //    عشان الـ [EnableRateLimiting] على الـ Endpoints يشتغل
        services.AddApiRateLimiting();

        // 6. التحقق الفاشل للإعدادات الحساسة: لازم تفشل عند بدء التشغيل
        //    بدل ما تبدأ بـ secret فاضي وتفشل جوه أول طلب.
        services.AddValidatedSecurityConfiguration(configuration, environment);

        return services;
    }
}

/// <summary>
/// يحذف كل الـ Controllers اللي تحت نطاق <c>Skill_Loop.Api.Controllers.Dev</c> من
/// الـ Application Model، فبتختفي من الـ Routing Table والـ Swagger نهائياً.
/// </summary>
internal sealed class DevControllerRemovalConvention : IApplicationModelConvention
{
    private const string DevNamespacePrefix = "Skill_Loop.Api.Controllers.Dev";

    public void Apply(ApplicationModel application)
    {
        var devControllers = application.Controllers
            .Where(c => c.ControllerType.Namespace?.StartsWith(DevNamespacePrefix, StringComparison.Ordinal) == true)
            .ToList();

        foreach (var controller in devControllers)
            application.Controllers.Remove(controller);
    }
}