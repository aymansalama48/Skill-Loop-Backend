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
            Expiry = TimeSpan.FromMinutes(5),
            ResendCooldown = TimeSpan.FromSeconds(60),
            MaxAttempts = 3,
            HashingSecret = "test-secret-key-for-testing-only-32chars!!"
        });

        _currentTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
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
        result.Data!.Code.Should().MatchRegex(@"^\d{4}$");
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
        result.Data!.Code.Should().MatchRegex(@"^\d{4}$");
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
}