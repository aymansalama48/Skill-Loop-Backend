namespace Skill_Loop.Application.Common.Abstractions.External.Cache;

public interface ICacheInvalidatorCommand
{
    IReadOnlyCollection<string> CacheKeys { get; }
}