using Serilog;
using Serilog.Events;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Api.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        // ✅ ضفنا السطر ده مؤقتاً عشان نشوف أي خطأ داخلي في الكونفيج
        //   Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine("SERILOG ERROR: " + msg));

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    public static IApplicationBuilder UseSerilogLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            // 1. تصنيف المستويات حسب الـ Status Code
            options.GetLevel = (httpContext, elapsed, ex) =>
                ex != null || httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode >= 400
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;

            // 2. إرفاق معرفات التتبع (Tracing Identifiers) للربط بين السجلات
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);

                if (httpContext.Items.TryGetValue(CorrelationConstants.HeaderKey, out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId);
                }
            };
        });
    }
}