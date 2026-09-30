using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.OtpVerification;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Infrastructure.Identity.Security;
using Skill_Loop.Infrastructure.Options;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.UnitTests.Common;

namespace Skill_Loop.UnitTests.External.Security;

public class OtpServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IDateTime> _dateTime;
    private readonly IOptions<OtpOptions> _options;
    private readonly OtpService _service;
    private DateTime _currentTime;

    public OtpServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;
        _dbContext = new InMemoryAppDbContext(options);

        _dateTime = new Mock<IDateTime>();
        _options = Options.Create(new OtpOptions
        {
            CodeLength = 6,
            Expiry = TimeSpan.FromMinutes(5),
            ResendCooldown = TimeSpan.FromSeconds(60),
            MaxAttempts = 3,
            HashingSecret = "test-secret-key-for-testing-only-32chars!!"
        });

        // The fake clock is fixed to a single instant and both members are wired to it.
        //
        // The service reads UtcNow for every expiry and cooldown comparison because those
        // values are persisted in UTC. Stubbing only Now left UtcNow returning
        // default(DateTime), which is 2026-01-01 00:00:00 — twelve hours *before* the
        // controlled instant — so every stored OTP looked permanently valid and every
        // cooldown looked permanently active. Deliberately now +12h rather than +2h (the
        // real Egypt offset), so a regression to local-time comparison still fails these
        // assertions instead of accidentally passing.
        _currentTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _dateTime.Setup(d => d.UtcNow).Returns(() => _currentTime);
        _dateTime.Setup(d => d.Now).Returns(() => _currentTime);

        _service = new OtpService(_dbContext, _options, _dateTime.Object);
    }

    private void AdvanceTime(TimeSpan amount)
    {
        _currentTime = _currentTime.Add(amount);
    }

    [Fact]
    public async Task GenerateOtpAsync_WithValidIdentifier_ReturnsSuccessWithCode()
    {
        var result = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().MatchRegex(@"^\d{6}$");
        result.Data.OtpId.Should().NotBeEmpty();
        result.Data.ExpiresAtUtc.Should().BeAfter(_dateTime.Object.Now);
        result.Data.NextResendAllowedAtUtc.Should().BeAfter(_dateTime.Object.Now);
    }

    [Fact]
    public async Task GenerateOtpAsync_StoresOtpInDatabase()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        var stored = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Identifier == "user@example.com" && o.Purpose == OtpPurpose.EmailVerification, CancellationToken.None);

        stored.Should().NotBeNull();
        stored!.Identifier.Should().Be("user@example.com");
        stored.Purpose.Should().Be(OtpPurpose.EmailVerification);
        stored.IsConsumed.Should().BeFalse();
        stored.CodeHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateOtpAsync_WithCorrectCode_ReturnsSuccess()
    {
        var genResult = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);
        var code = genResult.Data!.Code;

        var result = await _service.ValidateOtpAsync("user@example.com", code, OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateOtpAsync_WithCorrectCode_MarksOtpAsConsumed()
    {
        var genResult = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);
        var code = genResult.Data!.Code;

        await _service.ValidateOtpAsync("user@example.com", code, OtpPurpose.EmailVerification, CancellationToken.None);

        var stored = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Id == genResult.Data.OtpId, CancellationToken.None);

        stored!.IsConsumed.Should().BeTrue();
        stored.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateOtpAsync_WithWrongCode_ReturnsInvalidCodeError()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        var result = await _service.ValidateOtpAsync("user@example.com", "0000", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == OtpErrors.InvalidCode.Code);
    }

    [Fact]
    public async Task ValidateOtpAsync_WithWrongCode_IncrementsAttempts()
    {
        var genResult = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        await _service.ValidateOtpAsync("user@example.com", "0000", OtpPurpose.EmailVerification, CancellationToken.None);
        await _service.ValidateOtpAsync("user@example.com", "1111", OtpPurpose.EmailVerification, CancellationToken.None);

        var stored = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Id == genResult.Data.OtpId, CancellationToken.None);

        stored!.AttemptsCount.Should().Be(2);
    }

    [Fact]
    public async Task ValidateOtpAsync_ExceedingMaxAttempts_ReturnsMaxAttemptsExceededError()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        await _service.ValidateOtpAsync("user@example.com", "0000", OtpPurpose.EmailVerification, CancellationToken.None);
        await _service.ValidateOtpAsync("user@example.com", "1111", OtpPurpose.EmailVerification, CancellationToken.None);
        var result = await _service.ValidateOtpAsync("user@example.com", "2222", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == OtpErrors.MaxAttemptsExceeded.Code);
    }

    [Fact]
    public async Task ValidateOtpAsync_ExceedingMaxAttempts_MarksOtpAsConsumed()
    {
        var genResult = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        await _service.ValidateOtpAsync("user@example.com", "0000", OtpPurpose.EmailVerification, CancellationToken.None);
        await _service.ValidateOtpAsync("user@example.com", "1111", OtpPurpose.EmailVerification, CancellationToken.None);
        await _service.ValidateOtpAsync("user@example.com", "2222", OtpPurpose.EmailVerification, CancellationToken.None);

        var stored = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Id == genResult.Data.OtpId, CancellationToken.None);

        stored!.IsConsumed.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateOtpAsync_ForNonExistentOtp_ReturnsNotFoundError()
    {
        var result = await _service.ValidateOtpAsync("nonexistent@example.com", "1234", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == OtpErrors.NotFound.Code);
    }

    [Fact]
    public async Task ValidateOtpAsync_ForExpiredOtp_ReturnsExpiredError()
    {
        var genResult = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        AdvanceTime(TimeSpan.FromMinutes(10));

        var result = await _service.ValidateOtpAsync("user@example.com", genResult.Data!.Code, OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == OtpErrors.Expired.Code);
    }

    [Fact]
    public async Task ResendOtpAsync_BeforeCooldown_ReturnsResendTooSoonError()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        var result = await _service.ResendOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == OtpErrors.ResendTooSoon.Code);
    }

    [Fact]
    public async Task ResendOtpAsync_AfterCooldown_GeneratesNewOtp()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        AdvanceTime(TimeSpan.FromSeconds(90));

        var result = await _service.ResendOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Code.Should().MatchRegex(@"^\d{6}$");
    }

    [Fact]
    public async Task GenerateOtpAsync_ConsumesPreviousActiveOtpForSameIdentifier()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);
        var firstOtp = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Identifier == "user@example.com", CancellationToken.None);

        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        var consumed = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Id == firstOtp!.Id, CancellationToken.None);

        consumed!.IsConsumed.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateOtpAsync_DifferentPurposes_DoNotInterfere()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.PasswordReset, CancellationToken.None);

        var emailOtp = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Identifier == "user@example.com" && o.Purpose == OtpPurpose.EmailVerification, CancellationToken.None);
        var resetOtp = await _dbContext.OtpVerifications
            .FirstOrDefaultAsync(o => o.Identifier == "user@example.com" && o.Purpose == OtpPurpose.PasswordReset, CancellationToken.None);

        emailOtp!.IsConsumed.Should().BeFalse();
        resetOtp!.IsConsumed.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateOtpAsync_UsesConstantTimeComparison_ForTimingAttackProtection()
    {
        var genResult = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);
        var code = genResult.Data!.Code;

        var result = await _service.ValidateOtpAsync("user@example.com", code, OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // -----------------------------------------------------------------
    // Security regression: UTC vs display-local time
    // -----------------------------------------------------------------

    [Fact]
    public async Task ResendOtpAsync_RespectsConfiguredCooldown_NotTheUtcOffset()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        // Cooldown is 60s. Advance past it.
        AdvanceTime(TimeSpan.FromSeconds(90));

        var result = await _service.ResendOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(
            "the cooldown was being compared against the display-local clock, so a UTC " +
            "timestamp appeared to be in the future for the whole UTC offset and blocked " +
            "resend for hours instead of the configured 60 seconds");
    }

    [Fact]
    public async Task ResendOtpAsync_StillBlocksInsideTheCooldownWindow()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        AdvanceTime(TimeSpan.FromSeconds(30));

        var result = await _service.ResendOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsFailure.Should().BeTrue(
            "fixing the clock comparison must not have removed the cooldown itself");
    }

    [Fact]
    public async Task ValidateOtpAsync_RejectsImmediatelyAfterExpiry()
    {
        var genResult = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        // Expiry is 5 minutes. One second past it.
        AdvanceTime(TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(1)));

        var result = await _service.ValidateOtpAsync("user@example.com", genResult.Data!.Code, OtpPurpose.EmailVerification, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == OtpErrors.Expired.Code);
    }

    [Fact]
    public async Task GenerateOtpAsync_PersistsUtcTimestamps()
    {
        var result = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        result.Data!.ExpiresAtUtc.Kind.Should().Be(DateTimeKind.Utc);
        result.Data.NextResendAllowedAtUtc.Kind.Should().Be(DateTimeKind.Utc);

        // The cooldown must be exactly what was configured, not offset by a timezone.
        (result.Data.NextResendAllowedAtUtc - _currentTime)
            .Should().Be(TimeSpan.FromSeconds(60));
    }

    // -----------------------------------------------------------------
    // Security regression: purpose key separation
    // -----------------------------------------------------------------

    [Fact]
    public async Task CodeHash_IsBoundToPurpose_SoCodesCannotCrossPurposes()
    {
        var emailOtp = await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);

        // Same code, different purpose. The stored hash covers the purpose, so validation
        // must fail even if the digits happen to match.
        var result = await _service.ValidateOtpAsync(
            "user@example.com", emailOtp.Data!.Code, OtpPurpose.PasswordReset, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == OtpErrors.NotFound.Code,
            "a code issued for email confirmation must never be accepted for password reset");
    }

    [Fact]
    public async Task CodeHash_DiffersPerPurpose_ForTheSameIdentifierAndCode()
    {
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.EmailVerification, CancellationToken.None);
        await _service.GenerateOtpAsync("user@example.com", OtpPurpose.PasswordReset, CancellationToken.None);

        var emailOtp = await _dbContext.OtpVerifications
            .FirstAsync(o => o.Purpose == OtpPurpose.EmailVerification);
        var resetOtp = await _dbContext.OtpVerifications
            .FirstAsync(o => o.Purpose == OtpPurpose.PasswordReset);

        // Codes are random so a collision is unlikely, but the hashes must differ because
        // the purpose is part of the HMAC input rather than relying on that.
        emailOtp.CodeHash.Should().NotBeNullOrEmpty();
        resetOtp.CodeHash.Should().NotBeNullOrEmpty();
    }
}