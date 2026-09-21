namespace Skill_Loop.Application.Common.Abstractions.External.Client;

public interface IGeoLocationService
{
    Task<string?> GetLocationAsync(
        string? ipAddress,
        CancellationToken cancellationToken);
}