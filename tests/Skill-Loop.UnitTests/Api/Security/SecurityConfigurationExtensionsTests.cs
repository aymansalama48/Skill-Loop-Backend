namespace Skill_Loop.UnitTests.Api.Security;

using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Skill_Loop.Api.Extensions;

/// <summary>
/// Tests for startup-time security configuration validation.
///
/// Every secret in this application previously shipped with a working default committed to
/// appsettings.json, so the app started normally and nothing surfaced the problem until
/// someone read the repository. These tests pin the behaviour that a missing, short, or
/// previously-committed secret is now a startup failure instead.
/// </summary>
public class SecurityConfigurationExtensionsTests
{
    private const string ValidJwtKey = "a-valid-jwt-signing-key-of-at-least-32-bytes";
    private const string ValidOtpSecret = "a-valid-otp-hmac-secret-of-at-least-32-bytes";

    // The exact value that was committed to appsettings.json before the fix. It is in the
    // rejection list so a copy-pasted leftover cannot slip back in.
    private const string CompromisedOtpSecret =
        "YourSuper46854SecretOtpHashingSec56645retKeyHere_Minimum465645132Characters!";

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = ValidJwtKey,
            ["Jwt:Issuer"] = "https://localhost:7271",
            ["Jwt:Audience"] = "https://localhost:7271",
            ["Jwt:ExpiryMinutes"] = "60",
            ["OtpSettings:HashingSecret"] = ValidOtpSecret,
            ["OtpSettings:ResendCooldown"] = "00:02:00",
            ["CorsSettings:AllowedOrigins:0"] = "https://skillloop.example.com",
            ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=Skill-Loop;",
        };

        foreach (var (key, value) in overrides)
        {
            if (value is null)
                values.Remove(key);
            else
                values[key] = value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IWebHostEnvironment CreateEnvironment(string environmentName = "Production") =>
        new StubWebHostEnvironment { EnvironmentName = environmentName };

    private static void Validate(Dictionary<string, string?> overrides, string environmentName = "Production")
    {
        var services = new ServiceCollection();
        services.AddValidatedSecurityConfiguration(BuildConfiguration(overrides), CreateEnvironment(environmentName));
    }

    [Fact]
    public void ValidConfiguration_StartsSuccessfully()
    {
        var act = () => Validate([]);

        act.Should().NotThrow("a correct configuration must not block startup");
    }

    // -----------------------------------------------------------------
    // JWT signing key
    // -----------------------------------------------------------------

    [Fact]
    public void MissingJwtKey_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?> { ["Jwt:Key"] = null });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Key*");
    }

    [Fact]
    public void EmptyJwtKey_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?> { ["Jwt:Key"] = "" });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Key*");
    }

    [Fact]
    public void ShortJwtKey_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?> { ["Jwt:Key"] = "too-short" });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*at least 32*",
                "HMAC-SHA256 keys under 32 bytes are not safe to sign tokens with");
    }

    [Theory]
    [InlineData("360")]
    [InlineData("0")]
    [InlineData("-30")]
    public void ExcessiveOrInvalidJwtExpiry_FailsFast(string expiry)
    {
        var act = () => Validate(new Dictionary<string, string?> { ["Jwt:ExpiryMinutes"] = expiry });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:ExpiryMinutes*");
    }

    // -----------------------------------------------------------------
    // OTP hashing secret
    // -----------------------------------------------------------------

    [Fact]
    public void MissingOtpSecret_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?> { ["OtpSettings:HashingSecret"] = null });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*OtpSettings:HashingSecret*");
    }

    [Fact]
    public void PreviouslyCommittedOtpSecret_IsRejected()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["OtpSettings:HashingSecret"] = CompromisedOtpSecret
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*public*",
                "a secret that was committed to the repository must never be accepted again");
    }

    [Fact]
    public void PlaceholderOtpSecret_IsRejected()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["OtpSettings:HashingSecret"] = "YourSecretOtpHashingKeyGoesHere_1234567890"
        });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NearZeroResendCooldown_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["OtpSettings:ResendCooldown"] = "00:00:01"
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ResendCooldown*",
                "a near-zero cooldown lets one caller flood an inbox and burn SMTP quota");
    }

    [Fact]
    public void UnparseableResendCooldown_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["OtpSettings:ResendCooldown"] = "not-a-timespan"
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ResendCooldown*");
    }

    // -----------------------------------------------------------------
    // CORS
    // -----------------------------------------------------------------

    [Fact]
    public void EmptyCorsAllowlist_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["CorsSettings:AllowedOrigins:0"] = null
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CorsSettings*");
    }

    [Fact]
    public void WildcardCorsOrigin_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["CorsSettings:AllowedOrigins:0"] = "*"
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*wildcard*",
                "a wildcard combined with credentialed requests exposes authenticated responses to any site");
    }

    [Fact]
    public void LoopbackCorsOriginInProduction_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["CorsSettings:AllowedOrigins:0"] = "http://localhost:5173"
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*loopback*");
    }

    [Fact]
    public void LoopbackCorsOriginInDevelopment_IsAllowed()
    {
        var act = () => Validate(
            new Dictionary<string, string?> { ["CorsSettings:AllowedOrigins:0"] = "http://localhost:5173" },
            environmentName: "Development");

        act.Should().NotThrow("the Vite dev server legitimately runs on loopback HTTP");
    }

    [Fact]
    public void PlainHttpCorsOriginInProduction_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["CorsSettings:AllowedOrigins:0"] = "http://skillloop.example.com"
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*HTTP*");
    }

    // -----------------------------------------------------------------
    // Connection string
    // -----------------------------------------------------------------

    [Fact]
    public void MissingConnectionString_FailsFast()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = null
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DefaultConnection*");
    }

    [Fact]
    public void AllProblemsAreReportedTogether()
    {
        var act = () => Validate(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = null,
            ["OtpSettings:HashingSecret"] = null,
            ["CorsSettings:AllowedOrigins:0"] = "*"
        });

        // Reporting only the first problem would mean three deploy-fix-redeploy cycles.
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Key*")
            .WithMessage("*OtpSettings:HashingSecret*")
            .WithMessage("*wildcard*");
    }

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";

        public string ApplicationName { get; set; } = "Skill-Loop.Api";

        public string WebRootPath { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
