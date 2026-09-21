namespace Skill_Loop.Application.Common.Abstractions.External.Client.Models;

public sealed record ClientDeviceInfo(
    string Browser,
    string OperatingSystem,
    string DeviceType);