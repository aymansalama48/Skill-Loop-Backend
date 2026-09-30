namespace Skill_Loop.UnitTests.Features.Accounts.AccountManagement.Commands.ResetPassword;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

/// <summary>
/// Regression tests for the session-eviction fix in ResetPasswordCommandHandler.
///
/// The vulnerability: password reset changed the password but left every existing refresh
/// token valid for up to 7 days. An attacker holding a stolen refresh token could therefore
/// reset the victim's password — locking the legitimate user out — and remain fully
/// authenticated. The security control that makes password reset meaningful is session
/// revocation, so it is asserted directly here.
/// </summary>
public class ResetPasswordCommandHandlerTests
{
    private const string Email = "victim@example.com";

    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IUserManagementService> _userManagementService = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenService = new();
    private readonly Mock<IJobScheduler> _jobScheduler = new();
    private readonly Mock<IClientContext> _clientContext = new();

    private readonly Guid _userId = Guid.NewGuid();

    private ResetPasswordCommandHandler CreateSut()
    {
        _clientContext.Setup(c => c.IpAddress).Returns("203.0.113.10");
        _clientContext.Setup(c => c.UserAgent).Returns("test-agent");
        _jobScheduler
            .Setup(j => j.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<Task>>>()))
            .Verifiable();

        return new ResetPasswordCommandHandler(
            _otpService.Object,
            _passwordService.Object,
            _userManagementService.Object,
            _refreshTokenService.Object,
            _jobScheduler.Object,
            _clientContext.Object,
            Mock.Of<ILogger<ResetPasswordCommandHandler>>());
    }

    private void SetupHappyPath()
    {
        _otpService
            .Setup(s => s.ValidateOtpAsync(Email, "123456", OtpPurpose.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _passwordService
            .Setup(s => s.ResetPasswordAsync(Email, "NewPass@123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _userManagementService
            .Setup(s => s.GetUserByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto
            {
                Id = _userId,
                FirstName = "Victim",
                LastName = "User",
                FullName = "Victim User",
                Email = Email,
                IsActive = true
            }));
    }

    private static ResetPasswordCommand CreateCommand() =>
        new(Email, "123456", "NewPass@123", "NewPass@123");

    [Fact]
    public async Task Handle_RevokesAllSessionsAfterSuccessfulPasswordReset()
    {
        SetupHappyPath();
        _refreshTokenService
            .Setup(s => s.RevokeAllUserTokensAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("revoked"));

        var result = await CreateSut().Handle(CreateCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        // The core assertion: an existing refresh token must not survive a password reset.
        _refreshTokenService.Verify(
            s => s.RevokeAllUserTokensAsync(_userId, It.IsAny<CancellationToken>()),
            Times.Once,
            "a password reset that leaves old sessions alive is not a real account recovery");
    }

    [Fact]
    public async Task Handle_DoesNotRevokeSessionsWhenOtpValidationFails()
    {
        _otpService
            .Setup(s => s.ValidateOtpAsync(Email, "000000", OtpPurpose.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(OtpErrors.InvalidCode));

        var result = await CreateSut().Handle(
            new ResetPasswordCommand(Email, "000000", "NewPass@123", "NewPass@123"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _refreshTokenService.Verify(
            s => s.RevokeAllUserTokensAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "an attacker must not be able to log everyone out by submitting bad OTPs");
    }

    [Fact]
    public async Task Handle_DoesNotRevokeSessionsWhenPasswordChangeFails()
    {
        _otpService
            .Setup(s => s.ValidateOtpAsync(Email, "123456", OtpPurpose.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _passwordService
            .Setup(s => s.ResetPasswordAsync(Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(UserErrors.InvalidCredentials));

        var result = await CreateSut().Handle(CreateCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _refreshTokenService.Verify(
            s => s.RevokeAllUserTokensAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_StillSucceedsWhenSessionRevocationFails()
    {
        SetupHappyPath();
        _refreshTokenService
            .Setup(s => s.RevokeAllUserTokensAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("revoke failed"));

        var result = await CreateSut().Handle(CreateCommand(), CancellationToken.None);

        // The password is already changed, so reporting failure would tell the user their
        // recovery did not happen while it actually did. The handler logs the revocation
        // failure instead of swallowing or surfacing it as a command failure.
        result.IsSuccess.Should().BeTrue();
    }
}
