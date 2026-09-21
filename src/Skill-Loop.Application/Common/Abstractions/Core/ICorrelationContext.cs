namespace Skill_Loop.Application.Common.Abstractions.Core;

public interface ICorrelationContext
{
    string CorrelationId { get; }
}
