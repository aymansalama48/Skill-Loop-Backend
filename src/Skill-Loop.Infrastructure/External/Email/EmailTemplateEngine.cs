using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Skill_Loop.Infrastructure.External.Email;

public class EmailTemplateEngine
{
    public async Task<string> RenderTemplateAsync<TModel>(string templateName, TModel model)
    {
        var template = await LoadTemplateAsync(templateName);
        return ReplacePlaceholders(template, model);
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "External",
            "Email",
            "EmailTemplates",
            $"{templateName}.html");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"قالب البريد الإلكتروني '{templateName}' غير موجود.", templatePath);

        return await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
    }

    private string ReplacePlaceholders<TModel>(string template, TModel model)
    {
        if (model == null) return template;

        var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var values = new Dictionary<string, object>();

        foreach (var prop in properties)
        {
            var val = prop.GetValue(model);
            values[prop.Name] = val;
        }

        // 1. معالجة الشروط {{#if ...}} و {{else}} و {{/if}}
        template = ProcessConditionals(template, values);

        // 2. استبدال المتغيرات العادية مع دعم التنسيق {{Property:format}}
        template = Regex.Replace(template, @"\{\{(\w+)(?::([^}]+))?\}\}", match =>
        {
            var propName = match.Groups[1].Value;
            var format = match.Groups[2].Value;

            if (!values.ContainsKey(propName)) return string.Empty;

            var value = values[propName];

            // 💡 إذا كانت القيمة تاريخاً (DateTime أو DateTimeOffset)
            if (value is DateTime dateValue)
            {
                // لو لم يتم تحديد format، استخدم الصيغة الافتراضية بنظام 12 ساعة
                var effectiveFormat = !string.IsNullOrEmpty(format) ? format : "yyyy/MM/dd hh:mm tt";

                // يمكنك استخدام CultureInfo("ar-EG") لظهور (ص/م) أو CultureInfo.InvariantCulture لظهور (AM/PM)
                return dateValue.ToString(effectiveFormat, new CultureInfo("ar-EG"));
            }

            if (value is DateTimeOffset dateOffsetValue)
            {
                var effectiveFormat = !string.IsNullOrEmpty(format) ? format : "yyyy/MM/dd hh:mm tt";
                return dateOffsetValue.ToString(effectiveFormat, new CultureInfo("ar-EG"));
            }

            return value?.ToString() ?? string.Empty;
        });

        // 3. إزالة أي أقواس متبقية غير معروفة
        template = Regex.Replace(template, @"\{\{[^}]+\}\}", "");

        return template;
    }

    private string ProcessConditionals(string template, Dictionary<string, object> values)
    {
        var ifPattern = @"\{\{#if\s+([^}]+)\}\}(.*?)(?:\{\{else\}\}(.*?))?\{\{/if\}\}";
        var ifRegex = new Regex(ifPattern, RegexOptions.Singleline);

        var result = ifRegex.Replace(template, match =>
        {
            var propName = match.Groups[1].Value.Trim();
            var content = match.Groups[2].Value;
            var elseContent = match.Groups[3].Value;

            var isValid = values.ContainsKey(propName) && values[propName] != null && !string.IsNullOrWhiteSpace(values[propName].ToString());

            return isValid ? content : (string.IsNullOrEmpty(elseContent) ? string.Empty : elseContent);
        });

        var unlessPattern = @"\{\{#unless\s+([^}]+)\}\}(.*?)\{\{/unless\}\}";
        var unlessRegex = new Regex(unlessPattern, RegexOptions.Singleline);

        result = unlessRegex.Replace(result, match =>
        {
            var propName = match.Groups[1].Value.Trim();
            var content = match.Groups[2].Value;

            var isInvalid = !values.ContainsKey(propName) || values[propName] == null || string.IsNullOrWhiteSpace(values[propName].ToString());

            return isInvalid ? content : string.Empty;
        });

        return result;
    }
}
