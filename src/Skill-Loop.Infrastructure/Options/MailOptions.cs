namespace Skill_Loop.Infrastructure.Options;

public class MailOptions
{
    public const string SectionName = "MailSettings";

    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = "ClinicOS"; // اسم العيادة الثابت للنظام
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}
