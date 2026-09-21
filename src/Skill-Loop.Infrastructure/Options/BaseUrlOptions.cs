namespace Skill_Loop.Infrastructure.Options;

public sealed class BaseUrlOptions
{
    public const string SectionName = "BaseUrl";

    public string Backend { get; set; } = string.Empty;

    public string Frontend { get; set; } = string.Empty;
}