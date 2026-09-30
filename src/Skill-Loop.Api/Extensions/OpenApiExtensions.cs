using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Skill_Loop.Api.Extensions;

internal static class OpenApiSecurity
{
    public const string SchemeId = "BearerAuth";

    public static OpenApiSecurityRequirement Requirement(OpenApiDocument? host) => new()
    {
        [new OpenApiSecuritySchemeReference(SchemeId, host!)] = new List<string>()
    };
}

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // 1) components.securitySchemes
            //    بدون هذا السطر لا يعرض Scalar أي حقل للـ Authorization،
            //    فيضطر المستخدم للكتابة اليدوية في كل طلب على حدة.
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

            // 2) تمييز العمليات المحمية عن المجانية
            options.AddOperationTransformer<OperationSecurityTransformer>();
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();            // مسار ملف JSON: /openapi/v1.json
            app.MapScalarApiReference(options =>
            {
                // يبقي التوكن محفوظاً بين تحديثات الصفحة
                options.PersistentAuthentication = true;
            });
        }

        return app;
    }
}

/// <summary>
/// يضيف BearerAuth إلى components.securitySchemes ويجعله الافتراضي على مستوى المستند.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[OpenApiSecurity.SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Paste a JWT access token. Use POST /api/v1/Auth/staff/login for a token that carries " +
                "permissions, or POST /api/v1/Auth/user/login for an end-user token."
        };

        // الافتراضي: كل العمليات محمية، والـ Operation Transformer يستثني المجانية.
        document.Security ??= new List<OpenApiSecurityRequirement> { OpenApiSecurity.Requirement(document) };

        return Task.CompletedTask;
    }
}

/// <summary>
/// يضع securityRequirement على العمليات المحمية، ويضع security: [] على المجانية،
/// اعتماداً على [Authorize] و [AllowAnonymous] الفعلية على الـ action.
/// </summary>
internal sealed class OperationSecurityTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description?.ActionDescriptor?.EndpointMetadata;

        if (metadata is null)
        {
            // تعذّر قراءة الـ metadata: نُبقي الافتراضي (محمي) بدل تعطيل الحماية بالخطأ
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                OpenApiSecurity.Requirement(context.Document)
            };
            return Task.CompletedTask;
        }

        var allowAnonymous = metadata.Any(m => m is IAllowAnonymous);
        var requiresAuthorization = metadata.Any(m => m is IAuthorizeData);

        operation.Security = allowAnonymous || !requiresAuthorization
            ? Array.Empty<OpenApiSecurityRequirement>()
            : new List<OpenApiSecurityRequirement> { OpenApiSecurity.Requirement(context.Document) };

        return Task.CompletedTask;
    }
}
