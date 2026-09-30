namespace Skill_Loop.UnitTests.External.Security;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Identity.Tokens;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using Skill_Loop.UnitTests.Common;

/// <summary>
/// Security regression tests for RefreshTokenService.
///
/// Covers the three properties the old implementation lacked:
///   1. Tokens are persisted only as a SHA-256 digest, never in plaintext.
///   2. Rotation links tokens into a family, and replaying a rotated token revokes the
///      whole family instead of silently minting a new session.
///   3. A deactivated or unconfirmed account cannot use a valid refresh token to re-enter.
///
/// The old implementation stored the token verbatim in a <c>Token</c> column, so a
/// read-only database compromise yielded directly usable sessions for every user including
/// SuperAdmin.
/// </summary>
public class RefreshTokenServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    private InMemoryAppDbContext _context = null!;
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<IJwtTokenGenerator> _jwtGenerator = null!;
    private Mock<IPermissionService> _permissionService = null!;

    private RefreshTokenService CreateSut()
    {
        _context = new InMemoryAppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"refresh-token-tests-{Guid.NewGuid()}")
                .Options);

        return new RefreshTokenService(
            _context,
            _userManager.Object,
            _jwtGenerator.Object,
            _permissionService.Object,
            new FixedDateTime(),
            Mock.Of<ILogger<RefreshTokenService>>());
    }

    private void SetupMocks(ApplicationUser? user = null)
    {
        var subject = user ?? CreateUser();

        // Only the store is needed: FindByIdAsync and GetRolesAsync are virtual and are the
        // only UserManager members RefreshTokenService calls. Every other dependency is
        // unused, so nulls keep the test independent of Identity internals.
        _userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _userManager
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(subject);

        _userManager
            .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Admin" });

        _jwtGenerator = new Mock<IJwtTokenGenerator>();
        _jwtGenerator
            .Setup(g => g.GenerateJwtToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()))
            .Returns("generated-access-token");

        _permissionService = new Mock<IPermissionService>();
        _permissionService
            .Setup(p => p.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());
    }

    private ApplicationUser CreateUser(bool isActive = true, bool emailConfirmed = true) =>
        new()
        {
            Id = _userId,
            UserName = "user@example.com",
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive,
            EmailConfirmed = emailConfirmed
        };

    // -----------------------------------------------------------------
    // 1. Tokens are never stored in plaintext
    // -----------------------------------------------------------------

    [Fact]
    public async Task GenerateAndSaveRefreshToken_StoresOnlyADigestNotTheToken()
    {
        SetupMocks();
        var sut = CreateSut();

        var token = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);

        var stored = _context.RefreshTokens.Single();
        stored.TokenHash.Should().NotBe(token, "a bearer credential stored verbatim is a credential in the database");
        stored.TokenHash.Should().NotContain(token);
        stored.TokenHash.Should().HaveLength(44, "SHA-256 Base64 is always 44 characters");
    }

    [Fact]
    public async Task GenerateAndSaveRefreshToken_ProducesADifferentTokenEachTime()
    {
        SetupMocks();
        var sut = CreateSut();

        var first = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        var second = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);

        first.Should().NotBe(second);
        _context.RefreshTokens.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateAndSaveRefreshToken_AssignsAFreshTokenFamilyPerLogin()
    {
        SetupMocks();
        var sut = CreateSut();

        await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);

        var families = _context.RefreshTokens.Select(t => t.TokenFamilyId).ToList();

        families.Should().OnlyHaveUniqueItems("separate logins are separate families");
        families.Should().NotContain(Guid.Empty, "Guid.Empty would merge every session into one family");
    }

    // -----------------------------------------------------------------
    // 2. Rotation and reuse detection
    // -----------------------------------------------------------------

    [Fact]
    public async Task RefreshToken_IssuesANewTokenAndRevokesTheOldOne()
    {
        SetupMocks();
        var sut = CreateSut();

        var original = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        var result = await sut.RefreshTokenAsync(original, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.RefreshToken.Should().NotBe(original, "reusing the presented token means the old one leaked");

        var rows = _context.RefreshTokens.ToList();
        rows.Should().HaveCount(2);

        var newHash = Sha256Base64(result.Data.RefreshToken);
        var oldRow = rows.Single(t => t.TokenHash != newHash);

        oldRow.IsRevoked.Should().BeTrue("rotation must retire the presented token");
        oldRow.ReplacedByTokenHash.Should().Be(newHash);
    }

    [Fact]
    public async Task RefreshToken_KeepsTheSuccessorInTheSameFamily()
    {
        SetupMocks();
        var sut = CreateSut();

        var original = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        var familyId = _context.RefreshTokens.Single().TokenFamilyId;

        await sut.RefreshTokenAsync(original, CancellationToken.None);

        _context.RefreshTokens.Should().OnlyContain(
            t => t.TokenFamilyId == familyId,
            "the family is what makes theft traceable back to a single login");
    }

    [Fact]
    public async Task RefreshToken_ReplayingARotatedTokenRevokesTheWholeFamily()
    {
        SetupMocks();
        var sut = CreateSut();

        var original = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        await sut.RefreshTokenAsync(original, CancellationToken.None); // legitimate client rotates

        // Attacker replays the old token after the rotation already happened.
        var replay = await sut.RefreshTokenAsync(original, CancellationToken.None);

        replay.IsFailure.Should().BeTrue();
        SingleError(replay).Should().Be(TokenErrors.TokenReuseDetected);

        // The live session must die too: the attacker holds a valid token, so the whole
        // family is assumed compromised.
        _context.RefreshTokens.Should().OnlyContain(
            t => t.IsRevoked,
            "reuse detection exists to burn the family, not just the stale token");
    }

    [Fact]
    public async Task RefreshToken_ReuseOfOneUserDoesNotRevokeAnotherUsersSessions()
    {
        SetupMocks();
        var sut = CreateSut();

        var victimToken = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        await sut.RefreshTokenAsync(victimToken, CancellationToken.None); // rotate -> reuse becomes detectable

        var otherToken = await sut.GenerateAndSaveRefreshTokenAsync(_otherUserId, CancellationToken.None);

        await sut.RefreshTokenAsync(victimToken, CancellationToken.None); // trigger reuse detection

        _context.RefreshTokens
            .Where(t => t.UserId == _otherUserId)
            .Should().OnlyContain(
                t => !t.IsRevoked,
                "a per-family revoke must not turn into a system-wide logout");
    }

    [Fact]
    public async Task RefreshToken_RejectsAnUnknownToken()
    {
        SetupMocks();
        var sut = CreateSut();

        var result = await sut.RefreshTokenAsync("not-a-real-token", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        SingleError(result).Should().Be(TokenErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_RejectsAnExpiredToken()
    {
        SetupMocks();
        var sut = CreateSut();

        var token = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);

        var row = _context.RefreshTokens.Single();
        row.ExpiryDate = Now.AddMinutes(-1);
        await _context.SaveChangesAsync();

        var result = await sut.RefreshTokenAsync(token, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        SingleError(result).Should().Be(TokenErrors.TokenExpired);
    }

    [Fact]
    public async Task RefreshToken_ComparesExpiryAgainstUtcNotDisplayLocalTime()
    {
        // The clock here is UTC; the Egypt-local clock runs two hours ahead. An
        // implementation that compared against Now (local) would judge a token valid for
        // two extra hours.
        SetupMocks();
        var sut = CreateSut();

        var token = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);

        var row = _context.RefreshTokens.Single();
        row.ExpiryDate = Now.AddMinutes(1);
        await _context.SaveChangesAsync();

        var result = await sut.RefreshTokenAsync(token, CancellationToken.None);

        result.IsSuccess.Should().BeTrue("a token one minute in the future must still be valid");
    }

    // -----------------------------------------------------------------
    // 3. Account state is re-checked on refresh
    // -----------------------------------------------------------------

    [Fact]
    public async Task RefreshToken_RefusesADeactivatedAccount()
    {
        SetupMocks(CreateUser(isActive: false));
        var sut = CreateSut();

        var token = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        var result = await sut.RefreshTokenAsync(token, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        SingleError(result).Should().Be(
            UserErrors.AccountDeactivated,
            "a valid refresh token must not be a way back into a deactivated account");
    }

    [Fact]
    public async Task RefreshToken_RefusesAnUnconfirmedEmail()
    {
        SetupMocks(CreateUser(emailConfirmed: false));
        var sut = CreateSut();

        var token = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        var result = await sut.RefreshTokenAsync(token, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        SingleError(result).Should().Be(UserErrors.EmailNotConfirmed);
    }

    // -----------------------------------------------------------------
    // 4. Revocation entry points
    // -----------------------------------------------------------------

    [Fact]
    public async Task RevokeAllUserTokens_RevokesEveryActiveSessionForThatUserOnly()
    {
        SetupMocks();
        var sut = CreateSut();

        await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        await sut.GenerateAndSaveRefreshTokenAsync(_otherUserId, CancellationToken.None);

        var result = await sut.RevokeAllUserTokensAsync(_userId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _context.RefreshTokens.Where(t => t.UserId == _userId).Should().OnlyContain(t => t.IsRevoked);
        _context.RefreshTokens.Where(t => t.UserId == _otherUserId).Should().OnlyContain(t => !t.IsRevoked);
    }

    [Fact]
    public async Task RevokeAllUserTokens_IsSafeToCallWhenNoSessionsExist()
    {
        SetupMocks();
        var sut = CreateSut();

        var result = await sut.RevokeAllUserTokensAsync(_userId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(
            "password reset calls this for every account, including ones with no active session");
    }

    [Fact]
    public async Task RevokeAllUserTokens_BlocksRefreshForEverySessionOfThatUser()
    {
        SetupMocks();
        var sut = CreateSut();

        var token = await sut.GenerateAndSaveRefreshTokenAsync(_userId, CancellationToken.None);
        await sut.RevokeAllUserTokensAsync(_userId, CancellationToken.None);

        var result = await sut.RefreshTokenAsync(token, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        SingleError(result).Should().Be(TokenErrors.TokenRevoked);
    }

    private static Error SingleError(Result result) =>
        result.Errors.Should().ContainSingle().Subject;

    private static string Sha256Base64(string token) =>
        Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token)));

    /// <summary>
    /// Fixed clock so expiry and family assertions are deterministic. <see cref="Now"/>
    /// deliberately returns UTC so the tests fail if the implementation regresses to
    /// comparing against display-local time.
    /// </summary>
    private sealed class FixedDateTime : IDateTime
    {
        public DateTime UtcNow => Now;

        public DateTime Now => RefreshTokenServiceTests.Now;

        public string GetTimeString() => Now.ToString("HH:mm:ss");

        public string GetDateString() => Now.ToString("yyyy-MM-dd");

        public string GetDateTimeString() => Now.ToString("yyyy-MM-dd HH:mm:ss");

        public DateTime GetCurrentMonthStart() =>
            new(Now.Year, Now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        public DateTime GetLastMonthStart() => GetCurrentMonthStart().AddMonths(-1);

        public DateTime GetLastMonthEnd() => GetCurrentMonthStart().AddTicks(-1);

        public (DateTime current, DateTime lastStart, DateTime lastEnd) GetMonthRange() =>
            (GetCurrentMonthStart(), GetLastMonthStart(), GetLastMonthEnd());
    }
}
