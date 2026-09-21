using Skill_Loop.Application.Common.Abstractions.External.Client.Models;

namespace Skill_Loop.Application.Common.Abstractions.External.Client;

public interface IUserAgentParser
{
    ClientDeviceInfo Parse(string? userAgent);
}
