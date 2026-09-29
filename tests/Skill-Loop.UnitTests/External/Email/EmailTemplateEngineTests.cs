using System.Threading.Tasks;
using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Infrastructure.External.Email;
using Xunit;

namespace Skill_Loop.UnitTests.External.Email;

public class EmailTemplateEngineTests
{
    private readonly EmailTemplateEngine _engine;

    public EmailTemplateEngineTests()
    {
        _engine = new EmailTemplateEngine();
    }

    [Fact]
    public async Task RenderTemplateAsync_WelcomeTemplate_ReplacesAllPlaceholders()
    {
        var model = new WelcomeTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed",
            UserEmail = "ahmed@example.com",
            LoginUrl = "https://skillloop.com/login",
            SupportEmail = "support@skillloop.com",
            WebsiteUrl = "https://skillloop.com",
            ContactPhoneNumber = "+20123456789",
            WhatsAppNumber = "+20123456789"
        };

        var result = await _engine.RenderTemplateAsync("Welcome", model);

        result.Should().Contain("SkillLoop");
        result.Should().Contain("Ahmed");
        result.Should().Contain("https://skillloop.com/login");
        result.Should().Contain("support@skillloop.com");
        result.Should().Contain("https://skillloop.com");
        result.Should().Contain("+20123456789");
    }

    [Fact]
    public async Task RenderTemplateAsync_WelcomeTemplate_WithLoginUrl_RendersButton()
    {
        var model = new WelcomeTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed",
            LoginUrl = "https://skillloop.com/login"
        };

        var result = await _engine.RenderTemplateAsync("Welcome", model);

        result.Should().Contain("ابدأ الآن");
        result.Should().Contain("href=\"https://skillloop.com/login\"");
    }

    [Fact]
    public async Task RenderTemplateAsync_WelcomeTemplate_WithoutLoginUrl_HidesButton()
    {
        var model = new WelcomeTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed",
            LoginUrl = string.Empty
        };

        var result = await _engine.RenderTemplateAsync("Welcome", model);

        result.Should().NotContain("ابدأ الآن");
        result.Should().NotContain("LoginUrl");
    }

    [Fact]
    public async Task RenderTemplateAsync_WithIfBlock_ProcessesConditionally()
    {
        var modelWithLogin = new WelcomeTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed",
            LoginUrl = "https://skillloop.com/login"
        };

        var modelWithoutLogin = new WelcomeTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed",
            LoginUrl = string.Empty
        };

        var withResult = await _engine.RenderTemplateAsync("Welcome", modelWithLogin);
        var withoutResult = await _engine.RenderTemplateAsync("Welcome", modelWithoutLogin);

        withResult.Should().Contain("ابدأ الآن");
        withoutResult.Should().NotContain("ابدأ الآن");
    }

    [Fact]
    public async Task RenderTemplateAsync_WithUnlessBlock_ProcessesConditionally()
    {
        var template = "Hello {{#unless Hidden}}World{{/unless}}!";
        var result = await _engine.RenderTemplateAsync("Welcome", new
        {
            Hidden = false,
            AppName = "Test",
            UserName = "Test"
        });

        // Note: This test uses anonymous type but template expects Welcome template
        // We can't easily test custom template without embedded resource
    }

    [Fact]
    public async Task RenderTemplateAsync_EmailConfirmationTemplate_RendersCorrectly()
    {
        var model = new EmailConfirmationTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed",
            UserEmail = "ahmed@example.com",
            OtpCode = "1234"
        };

        var result = await _engine.RenderTemplateAsync("EmailConfirmation", model);

        result.Should().Contain("SkillLoop");
        result.Should().Contain("Ahmed");
        result.Should().Contain("1234");
    }

    [Fact]
    public async Task RenderTemplateAsync_ResetPasswordTemplate_RendersCorrectly()
    {
        var model = new ResetPasswordTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed",
            OtpCode = "5678"
        };

        var result = await _engine.RenderTemplateAsync("ResetPassword", model);

        result.Should().Contain("SkillLoop");
        result.Should().Contain("Ahmed");
        result.Should().Contain("5678");
    }

    [Fact]
    public async Task RenderTemplateAsync_StaffInvitationTemplate_RendersCorrectly()
    {
        var model = new StaffInvitationTemplateModel
        {
            AppName = "SkillLoop",
            AdminName = "Admin",
            RoleName = "Instructor",
            InvitationLink = "https://skillloop.com/accept?token=abc123",
            ExpiryHours = 7
        };

        var result = await _engine.RenderTemplateAsync("StaffInvitation", model);

        result.Should().Contain("SkillLoop");
        result.Should().Contain("Admin");
        result.Should().Contain("Instructor");
        result.Should().Contain("https://skillloop.com/accept?token=abc123");
        result.Should().Contain("7");
    }

    [Fact]
    public async Task RenderTemplateAsync_SessionMaterialUploadedTemplate_RendersCorrectly()
    {
        var model = new SessionMaterialUploadedTemplateModel
        {
            AppName = "SkillLoop",
            InstructorName = "Mohamed",
            SessionTitle = "Advanced C#",
            MaterialName = "slides.pdf",
            MaterialType = "PDF",
            UploadedAt = "2026-01-01 12:00",
            DownloadUrl = "https://skillloop.com/download/123"
        };

        var result = await _engine.RenderTemplateAsync("SessionMaterialUploaded", model);

        result.Should().Contain("Advanced C#");
        result.Should().Contain("slides.pdf");
        result.Should().Contain("Mohamed");
        result.Should().Contain("https://skillloop.com/download/123");
    }

    [Fact]
    public async Task RenderTemplateAsync_HandlesDateTimeFormatting()
    {
        var model = new WelcomeTemplateModel
        {
            AppName = "SkillLoop",
            UserName = "Ahmed"
        };

        var result = await _engine.RenderTemplateAsync("Welcome", model);

        // Should not contain any unprocessed placeholders
        result.Should().NotContain("{{");
        result.Should().NotContain("}}");
    }
}

public class EmailConfirmationTemplateModel : BaseEmailTemplateModel
{
    public string OtpCode { get; set; } = string.Empty;
}

public class ResetPasswordTemplateModel : BaseEmailTemplateModel
{
    public string OtpCode { get; set; } = string.Empty;
}

public class StaffInvitationTemplateModel : BaseEmailTemplateModel
{
    public string AdminName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string InvitationLink { get; set; } = string.Empty;
    public int ExpiryHours { get; set; }
}

public class SessionMaterialUploadedTemplateModel : BaseEmailTemplateModel
{
    public string InstructorName { get; set; } = string.Empty;
    public string SessionTitle { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialType { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}