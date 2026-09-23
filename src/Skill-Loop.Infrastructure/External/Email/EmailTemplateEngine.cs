using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Skill_Loop.Infrastructure.External.Email;

/// <summary>
/// محرك قوالب بسيط يقرأ قوالب HTML من الـ Embedded Resources،
/// ويدعم المتغيرات {{Property}} و {{Property:format}}،
/// والشروط {{#if Property}}...{{else}}...{{/if}} و {{#unless Property}}...{{/unless}}.
/// </summary>
public class EmailTemplateEngine
{
    private readonly Assembly _assembly;

    public EmailTemplateEngine()
    {
        // نجيب نفس الـ Assembly اللي فيه الـ Engine (الـ Infrastructure)
        _assembly = typeof(EmailTemplateEngine).Assembly;
    }

    // ============================================================
    // الواجهة العامة
    // ============================================================

    public async Task<string> RenderTemplateAsync<TModel>(string templateName, TModel model)
    {
        var template = await LoadTemplateAsync(templateName);
        return ReplacePlaceholders(template, model);
    }

    // ============================================================
    // تحميل القالب من الـ Embedded Resource
    // ============================================================

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        // نبحث عن أي Resource اسمه بينتهي بـ .EmailTemplates.{templateName}.html
        // البحث حساس لحالة الأحرف بالنسبة لاسم القالب، وغير حساس للـ namespace prefix
        var suffix = $".EmailTemplates.{templateName}.html";

        var resourceName = _assembly.GetManifestResourceNames()
            .FirstOrDefault(n =>
                n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(resourceName))
        {
            // رسالة واضحة تساعد في الديباج، فيها كل الأسماء المتاحة
            var available = string.Join(", ", _assembly.GetManifestResourceNames());
            throw new FileNotFoundException(
                $"قالب البريد الإلكتروني '{templateName}' غير موجود كـ Embedded Resource. " +
                $"الأسماء المتاحة: [{available}]",
                suffix);
        }

        using var stream = _assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"تعذّر فتح قالب البريد الإلكتروني '{templateName}' من الـ Assembly (resourceName: {resourceName}).");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    // ============================================================
    // استبدال الـ Placeholders
    // ============================================================

    private string ReplacePlaceholders<TModel>(string template, TModel model)
    {
        if (model is null) return template;

        var properties = typeof(TModel)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in properties)
        {
            values[prop.Name] = prop.GetValue(model);
        }

        // 1. معالجة {{#unless ...}} أولاً (قبل {{#if}} عشان لو فيه تداخل)
        template = ProcessUnlessBlocks(template, values);

        // 2. معالجة {{#if ...}} ... {{else}} ... {{/if}}
        template = ProcessIfBlocks(template, values);

        // 3. استبدال المتغيرات العادية مع دعم التنسيق {{Property:format}}
        template = Regex.Replace(
            template,
            @"\{\{(\w+)(?::([^}]+))?\}\}",
            match =>
            {
                var propName = match.Groups[1].Value;
                var format = match.Groups[2].Value;

                if (!values.TryGetValue(propName, out var value) || value is null)
                    return string.Empty;

                // DateTime / DateTimeOffset — تنسيق موحّد بالعربي
                if (value is DateTime dateValue)
                {
                    var effectiveFormat = !string.IsNullOrEmpty(format)
                        ? format
                        : "yyyy/MM/dd hh:mm tt";

                    return dateValue.ToString(effectiveFormat, new CultureInfo("ar-EG"));
                }

                if (value is DateTimeOffset dateOffsetValue)
                {
                    var effectiveFormat = !string.IsNullOrEmpty(format)
                        ? format
                        : "yyyy/MM/dd hh:mm tt";

                    return dateOffsetValue.ToString(effectiveFormat, new CultureInfo("ar-EG"));
                }

                return value.ToString() ?? string.Empty;
            });

        // 4. إزالة أي placeholders متبقية غير معروفة
        template = Regex.Replace(template, @"\{\{[^}]+\}\}", string.Empty);

        return template;
    }

    // ============================================================
    // فحص "الحقيقة" للقيمة — Generic بحت بدون قائمة استبعاد
    // ============================================================
    //
    // القاعدة الوحيدة: القيمة falsy لو كانت null أو نص فاضي/مسافات.
    // أي حاجة تانية (بما فيها 0، false كـ bool، enum default) تُعتبر truthy.
    // السبب: "0" قد تكون قيمة حقيقية (نسبة خصم، عدد محاولات، إلخ).
    //
    private static bool IsTruthy(object? value)
    {
        if (value is null) return false;

        // النصوص: نرفض الفاضي والمسافات
        if (value is string s) return !string.IsNullOrWhiteSpace(s);

        // أي نوع تاني: نعتبره truthy
        return true;
    }

    // ============================================================
    // معالجة {{#if}} ... {{else}} ... {{/if}}
    // ============================================================

    private string ProcessIfBlocks(string template, Dictionary<string, object?> values)
    {
        // نستخدم Negative Lookahead عشان نتجنب ابتلاع {{#if}} الداخلية
        // ونكرر العملية لحد ما يحصل استقرار (يدعم التداخل بمستويات متعددة)
        var pattern =
            @"\{\{#if\s+(\w+)\s*\}\}" +
            @"((?:(?!\{\{#if).)*?)" +
            @"(?:\{\{else\}\}((?:(?!\{\{#if).)*?))?" +
            @"\{\{/if\}\}";

        var regex = new Regex(pattern, RegexOptions.Singleline);
        var result = template;

        string previous;
        do
        {
            previous = result;

            result = regex.Replace(result, match =>
            {
                var propName = match.Groups[1].Value.Trim();
                var trueContent = match.Groups[2].Value;
                var falseContent = match.Groups[3].Success
                    ? match.Groups[3].Value
                    : string.Empty;

                var isTruthy = values.TryGetValue(propName, out var value)
                               && IsTruthy(value);

                return isTruthy ? trueContent : falseContent;
            });
        }
        while (result != previous);

        return result;
    }

    // ============================================================
    // معالجة {{#unless}} ... {{/unless}}
    // ============================================================

    private string ProcessUnlessBlocks(string template, Dictionary<string, object?> values)
    {
        var pattern =
            @"\{\{#unless\s+(\w+)\s*\}\}" +
            @"((?:(?!\{\{#unless).)*?)" +
            @"\{\{/unless\}\}";

        var regex = new Regex(pattern, RegexOptions.Singleline);
        var result = template;

        string previous;
        do
        {
            previous = result;

            result = regex.Replace(result, match =>
            {
                var propName = match.Groups[1].Value.Trim();
                var content = match.Groups[2].Value;

                var isFalsy = !values.TryGetValue(propName, out var value)
                              || !IsTruthy(value);

                return isFalsy ? content : string.Empty;
            });
        }
        while (result != previous);

        return result;
    }
}