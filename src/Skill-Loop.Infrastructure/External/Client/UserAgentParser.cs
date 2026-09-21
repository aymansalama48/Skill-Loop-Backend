using Skill_Loop.Application.Common.Abstractions.External.Client;
using Skill_Loop.Application.Common.Abstractions.External.Client.Models;
using UAParser;

namespace Skill_Loop.Infrastructure.External.Client;

/// <summary>
/// خدمة تحليل نص الـ User-Agent واستخراج معالم الجهاز والمتصفح باستخدام مكتبة UAParser
/// </summary>
public sealed class UserAgentParser : IUserAgentParser
{
    // تحميل قواعد القراءة مرة واحدة للأداء العالي (Thread-safe)
    private static readonly Parser _parser = Parser.GetDefault();

    public ClientDeviceInfo Parse(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return new ClientDeviceInfo("متصفح غير معروف", "نظام غير معروف", "جهاز غير معروف");
        }

        // تحليل النص بواسطة مكتبة UAParser
        var clientInfo = _parser.Parse(userAgent);

        // 1. استخراج المتصفح والإصدار
        var browserFamily = clientInfo.UA.Family;
        var browser = !string.IsNullOrWhiteSpace(browserFamily) && browserFamily != "Other"
            ? $"{browserFamily} {clientInfo.UA.Major}".Trim()
            : "متصفح غير معروف";

        // 2. استخراج نظام التشغيل والإصدار
        var osFamily = clientInfo.OS.Family;
        var os = !string.IsNullOrWhiteSpace(osFamily) && osFamily != "Other"
            ? $"{osFamily} {clientInfo.OS.Major}".Trim()
            : "نظام غير معروف";

        // 3. استخراج نوع الجهاز (Desktop / Mobile / Tablet)
        var deviceFamily = clientInfo.Device.Family;
        var deviceType = !string.IsNullOrWhiteSpace(deviceFamily) && deviceFamily != "Other"
            ? deviceFamily
            : (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ? "هاتف محمول (Mobile)" : "حاسوب (Desktop)");

        return new ClientDeviceInfo(browser, os, deviceType);
    }
}