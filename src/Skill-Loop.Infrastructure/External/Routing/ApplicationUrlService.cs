using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.Routing;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.External.Routing;

public sealed class ApplicationUrlService : IApplicationUrlService
{
    private readonly BaseUrlOptions _options;

    public ApplicationUrlService(
        IOptions<BaseUrlOptions> options)
    {
        _options = options.Value;
    }

    public string GeneratePasswordResetUrl(
        string email,
        string token)
    {
        return string.Concat(
            _options.Frontend.TrimEnd('/'),
            "/reset-password",
            "?email=",
            Uri.EscapeDataString(email),
            "&token=",
            Uri.EscapeDataString(token));
    }

    // 👈 تنفيذ الدالة الجديدة
    public string GenerateStaffInvitationUrl(string token)
    {
        // يمكنك تغيير مسار "/accept-invitation" ليتطابق مع المسار الحقيقي في الواجهة الأمامية (React/Angular)
        return string.Concat(
            _options.Frontend.TrimEnd('/'),
            "/accept-invitation",
            "?token=",
            Uri.EscapeDataString(token));
    }
}