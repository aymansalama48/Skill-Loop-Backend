namespace Skill_Loop.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    // المفتاح السري اللي بيتم توقيع التوكن بيه (لازم يكون طويل وقوي ومحفوظ بأمان)
    public string Key { get; set; } = string.Empty;

    // اسم الجهة اللي أصدرت التوكن (عادة اسم موقعك أو الـ API)
    public string Issuer { get; set; } = string.Empty;

    // اسم الجهة اللي التوكن موجه ليها (عادة الفرونت إند أو الموبايل أب)
    public string Audience { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;


}