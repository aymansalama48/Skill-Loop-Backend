using Hangfire.Dashboard;

namespace Skill_Loop.Api.Extensions;

public class HangfireCustomAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // السماح بالوصول دائماً في بيئة التطوير (Development)
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            return true;
        }

        // في الإنتاج (Production): التأكد من تسجيل الدخول ويمتلك دور Admin
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}