using Microsoft.AspNetCore.Http;
using Skill_Loop.Application.Common.Abstractions.Web;

namespace Skill_Loop.Infrastructure.External.Client;


/// <summary>
/// خدمة استخراج بيانات العميل من سياق الطلب الحالي (HttpContext)
/// </summary>
public sealed class HttpClientContext : IClientContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpClientContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? IpAddress
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // جلب IP العميل الحقيقي حتى لو كان خلف Nginx / Cloudflare
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                var clientIp = forwardedFor.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(clientIp))
                    return clientIp;
            }

            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
                return realIp.Trim();

            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
            return remoteIp == "::1" ? "127.0.0.1" : remoteIp;
        }
    }

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}