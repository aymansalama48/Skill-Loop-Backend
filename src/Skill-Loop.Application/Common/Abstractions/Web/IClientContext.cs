namespace Skill_Loop.Application.Common.Abstractions.Web;

public interface IClientContext
{
    string? IpAddress { get; }

    string? UserAgent { get; }
}
