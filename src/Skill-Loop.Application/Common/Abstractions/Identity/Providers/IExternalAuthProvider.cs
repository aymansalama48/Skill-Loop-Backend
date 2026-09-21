using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Providers;

public interface IExternalAuthProvider
{
    string ProviderName { get; }

    Task<Result<ExternalUserResult>> ValidateTokenAsync(
        string idToken,
        CancellationToken cancellationToken);
}
