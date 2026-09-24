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
        IConfiguration configuration)
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

        // 2.5 الشات اللحظي (SignalR)
        services.AddChatRealtime();

        // 3. إضافة دعم الـ ProblemDetails والـ Global Exception Handler
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // 4. الصلاحيات والتوثيق المكتبي والـ CORS
        services.AddAuthorization();
        services.AddOpenApiDocumentation();
        services.AddCorsPolicy(configuration);

        return services;
    }
}