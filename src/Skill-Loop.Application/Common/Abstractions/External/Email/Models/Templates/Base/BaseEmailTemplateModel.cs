namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;

public abstract class BaseEmailTemplateModel
{
    // خصائص عامة لكل القوالب
    public string AppName { get; set; } = string.Empty; // تم تغيير من ClinicName
    public string SupportEmail { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty; // تم تغيير من ClinicPhoneNumber
    public string Address { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string FacebookUrl { get; set; } = string.Empty;
    public string InstagramUrl { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;

    // خصائص أساسية للمستخدم
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}