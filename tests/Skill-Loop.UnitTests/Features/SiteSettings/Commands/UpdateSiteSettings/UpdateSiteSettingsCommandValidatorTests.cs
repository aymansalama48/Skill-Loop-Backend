using System;
using System.Threading;
using FluentAssertions;
using Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;
using Xunit;

namespace Skill_Loop.UnitTests.Features.SiteSettings.Commands.UpdateSiteSettings;

public class UpdateSiteSettingsCommandValidatorTests
{
    private readonly UpdateSiteSettingsCommandValidator _validator;

    public UpdateSiteSettingsCommandValidatorTests()
    {
        _validator = new UpdateSiteSettingsCommandValidator();
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "SkillLoop",
            LogoName: "logo.png",
            SupportEmail: "support@skillloop.com",
            ContactPhoneNumber: "+20123456789",
            Address: "Cairo, Egypt",
            WebsiteUrl: "https://skillloop.com",
            FacebookUrl: "https://facebook.com/skillloop",
            InstagramUrl: "https://instagram.com/skillloop",
            WhatsAppNumber: "+20123456789"
        );

        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_InvalidSupportEmail_ReturnsError()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "SkillLoop",
            LogoName: null,
            SupportEmail: "invalid-email",
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: null,
            FacebookUrl: null,
            InstagramUrl: null,
            WhatsAppNumber: null
        );

        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Invalid value."));
    }

    [Fact]
    public async Task Validate_InvalidWebsiteUrl_ReturnsError()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "SkillLoop",
            LogoName: null,
            SupportEmail: null,
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: "not-a-valid-url",
            FacebookUrl: null,
            InstagramUrl: null,
            WhatsAppNumber: null
        );

        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Invalid value."));
    }

    [Fact]
    public async Task Validate_InvalidFacebookUrl_ReturnsError()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "SkillLoop",
            LogoName: null,
            SupportEmail: null,
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: null,
            FacebookUrl: "not-a-valid-url",
            InstagramUrl: null,
            WhatsAppNumber: null
        );

        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Invalid value."));
    }

    [Fact]
    public async Task Validate_InvalidInstagramUrl_ReturnsError()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "SkillLoop",
            LogoName: null,
            SupportEmail: null,
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: null,
            FacebookUrl: null,
            InstagramUrl: "not-a-valid-url",
            WhatsAppNumber: null
        );

        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Invalid value."));
    }

    [Fact]
    public async Task Validate_NullOrEmptyUrls_PassesValidation()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "SkillLoop",
            LogoName: null,
            SupportEmail: null,
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: "",
            FacebookUrl: null,
            InstagramUrl: "",
            WhatsAppNumber: null
        );

        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ValidUrls_PassesValidation()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "SkillLoop",
            LogoName: null,
            SupportEmail: "test@test.com",
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: "https://example.com",
            FacebookUrl: "https://facebook.com/page",
            InstagramUrl: "https://instagram.com/profile",
            WhatsAppNumber: null
        );

        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }
}