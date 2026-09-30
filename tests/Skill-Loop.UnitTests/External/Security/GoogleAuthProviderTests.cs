using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.Identity.Providers;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Infrastructure.Identity.Providers;
using Skill_Loop.Infrastructure.Options;
using Xunit;

namespace Skill_Loop.UnitTests.External.Security;

public class GoogleAuthProviderTests
{
    private static GoogleAuthProvider Build(string clientId)
        => new(
            Options.Create(new GoogleAuthOptions { ClientId = clientId }),
            NullLogger<GoogleAuthProvider>.Instance);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateTokenAsync_WhenClientIdMissing_FailsClosed(string clientId)
    {
        // Without an audience Google only checks the signature/issuer, so a token minted for a
        // different app would be accepted. The provider must refuse instead.
        var provider = Build(clientId);

        var result = await provider.ValidateTokenAsync("some.jwt.token", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ExternalAuthErrors.InvalidToken.Code);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithMalformedToken_ReturnsInvalidToken()
    {
        var provider = Build("configured-client-id.apps.googleusercontent.com");

        var result = await provider.ValidateTokenAsync("not-a-jwt", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == ExternalAuthErrors.InvalidToken.Code);
    }

    [Fact]
    public void ProviderName_IsGoogle()
    {
        Build("configured-client-id").ProviderName.Should().Be("Google");
    }
}