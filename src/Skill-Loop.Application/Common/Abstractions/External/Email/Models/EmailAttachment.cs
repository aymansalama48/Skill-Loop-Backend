namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models;

public class EmailAttachment
{
    // اسم الملف
    public string FileName { get; set; } = string.Empty;

    // محتوى الملف
    public Stream Content { get; set; } = Stream.Null;

    // نوع الملف
    public string ContentType { get; set; } = "application/octet-stream";
}