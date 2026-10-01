using Hangfire;
using Microsoft.Extensions.FileProviders;
using Skill_Loop.Api.Middlewares;
using Skill_Loop.Infrastructure.BackgroundJobs;

namespace Skill_Loop.Api.Extensions;

public static class PipelineExtensions
{
    /// <summary>
    /// تطبيق خط سير الطلبات (Request Pipeline) بترتيبه الصحيح
    /// </summary>
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {





        // 1. تشغيل Correlation ID في أسرع نقطة دخول للطلب لتتبع الـ Requests
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 2. هيدرات الأمان: nosniff / frame-options / CSP على كل الاستجابات
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // 3. استخراج هوية الطلب (الإيميل) من الـ Body قبل الـ Rate Limiter
        //    لازم يسبق الـ Rate Limiter لأن الـ Limiter بيقسم الـ Buckets على أساسه،
        //    ولازم يعمل buffering عشان الـ Model Binding يفضل يقرأ الـ Body عادي.
        app.UseMiddleware<RateLimitIdentityMiddleware>();

        // 4. Rate Limiting: بعد الـ Identity, وقبل الـ Routing/Authorization
        //    لأن الـ Policies المطبقة على الـ Endpoints محتاجة الـ Endpoint metadata.
        app.UseRateLimiter();

        // 5. معالجة الاستثناءات وتسجيل الـ Requests عبر Serilog بالخيارات المخصصة
        app.UseExceptionHandler();
        app.UseSerilogLogging(); // 👈 استبدال app.UseSerilogRequestLogging() هنا

        // 6. التوجيه الآمن والـ CORS
        app.UseHsts(); // 👈 مطلوب مع UseHttpsRedirection في الإنتاج
        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");

        // 7. Serving the uploaded-files folder (Static Files)
        // The root comes from FileStorage:RootFolder, defaulting to "UploadedFiles".
        // It may be an absolute path (a Docker volume mount), so it is resolved with
        // GetFullPath rather than combined blindly. Kept from the security branch during
        // the merge: origin/main concatenated unconditionally, which mishandled rooted paths.
        var storageRootFolder = app.Configuration["FileStorage:RootFolder"];
        if (string.IsNullOrWhiteSpace(storageRootFolder))
        {
            storageRootFolder = "UploadedFiles";
        }

        var uploadsPath = Path.GetFullPath(
            Path.IsPathRooted(storageRootFolder)
                ? storageRootFolder
                : Path.Combine(app.Environment.ContentRootPath, storageRootFolder));

        Directory.CreateDirectory(uploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/UploadedFiles"  // الـ URL ثابت، مش بتغير
        });

        // 8. توثيق OpenAPI/Scalar
        app.UseOpenApiDocumentation();

        // 9. التوثيق والصلاحيات
        app.UseAuthentication();
        app.UseAuthorization();


        // 10. تفعيل شاشة Hangfire للمراقبة
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireCustomAuthorizationFilter() }
        });
        RecurringJob.AddOrUpdate<ProcessOutboxMessagesJob>(
            "process-outbox-messages",
            job => job.ProcessAsync(),
            "*/5 * * * * *"); // Cron Expression للتكرار كل 5 ثوانٍ

        RecurringJob.AddOrUpdate<RefreshDriveQuotaJob>(
            "refresh-drive-quota",
            job => job.RefreshAsync(),
            "0 */12 * * *"); // كل 12 ساعة

        RecurringJob.AddOrUpdate<ProcessPendingEmailsJob>(
            "process-pending-emails",
            job => job.ProcessAsync(),
            "* * * * *"); // كل دقيقة


        // 11. ربط الـ Controllers
        app.MapControllers();

        // 12. الشات اللحظي (SignalR Hub) على /hubs/chat
        app.MapChatHub();

        return app;
    }
}
