using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;
using Skill_Loop.Domain.Common.Results;
using SiteSettingsEntity = Skill_Loop.Domain.Entities.SiteSettings.SiteSettings;
using Skill_Loop.UnitTests.Common;
using Xunit;

namespace Skill_Loop.UnitTests.Features.SiteSettings.Commands.UpdateSiteSettings;

public class UpdateSiteSettingsCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly UpdateSiteSettingsCommandHandler _handler;

    public UpdateSiteSettingsCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new UpdateSiteSettingsCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_NoExistingSettings_CreatesNewSettings()
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

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        var settings = await _dbContext.FirstOrDefaultAsync(_dbContext.SiteSettings, CancellationToken.None);
        settings.Should().NotBeNull();
        settings!.AppName.Should().Be("SkillLoop");
        settings.LogoName.Should().Be("logo.png");
        settings.SupportEmail.Should().Be("support@skillloop.com");
        settings.ContactPhoneNumber.Should().Be("+20123456789");
        settings.Address.Should().Be("Cairo, Egypt");
        settings.WebsiteUrl.Should().Be("https://skillloop.com");
        settings.FacebookUrl.Should().Be("https://facebook.com/skillloop");
        settings.InstagramUrl.Should().Be("https://instagram.com/skillloop");
        settings.WhatsAppNumber.Should().Be("+20123456789");
    }

    [Fact]
    public async Task Handle_ExistingSettings_UpdatesOnlyProvidedFields()
    {
        var existing = new SiteSettingsEntity
        {
            AppName = "Old Name",
            LogoName = "old-logo.png",
            SupportEmail = "old@support.com",
            ContactPhoneNumber = "+20111111111",
            Address = "Old Address",
            WebsiteUrl = "https://old.com",
            FacebookUrl = "https://facebook.com/old",
            InstagramUrl = "https://instagram.com/old",
            WhatsAppNumber = "+20111111111"
        };
        _dbContext.Add(existing);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateSiteSettingsCommand(
            AppName: "New Name",
            LogoName: null, // should keep old
            SupportEmail: "new@support.com",
            ContactPhoneNumber: null, // should keep old
            Address: null,
            WebsiteUrl: null,
            FacebookUrl: null,
            InstagramUrl: null,
            WhatsAppNumber: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var settings = await _dbContext.FirstOrDefaultAsync(_dbContext.SiteSettings, CancellationToken.None);
        settings!.AppName.Should().Be("New Name");
        settings.LogoName.Should().Be("old-logo.png"); // unchanged
        settings.SupportEmail.Should().Be("new@support.com");
        settings.ContactPhoneNumber.Should().Be("+20111111111"); // unchanged
        settings.Address.Should().Be("Old Address"); // unchanged
        settings.WebsiteUrl.Should().Be("https://old.com"); // unchanged
        settings.FacebookUrl.Should().Be("https://facebook.com/old"); // unchanged
        settings.InstagramUrl.Should().Be("https://instagram.com/old"); // unchanged
        settings.WhatsAppNumber.Should().Be("+20111111111"); // unchanged
    }

    [Fact]
    public async Task Handle_EmptyCommand_KeepsAllExistingValues()
    {
        var existing = new SiteSettingsEntity
        {
            AppName = "Existing Name",
            SupportEmail = "existing@support.com"
        };
        _dbContext.Add(existing);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateSiteSettingsCommand(
            AppName: null,
            LogoName: null,
            SupportEmail: null,
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: null,
            FacebookUrl: null,
            InstagramUrl: null,
            WhatsAppNumber: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var settings = await _dbContext.FirstOrDefaultAsync(_dbContext.SiteSettings, CancellationToken.None);
        settings!.AppName.Should().Be("Existing Name");
        settings.SupportEmail.Should().Be("existing@support.com");
    }

    [Fact]
    public async Task Handle_ImplementsCacheInvalidatorCommand_ReturnsCacheKeys()
    {
        var command = new UpdateSiteSettingsCommand(
            AppName: "Test",
            LogoName: null,
            SupportEmail: null,
            ContactPhoneNumber: null,
            Address: null,
            WebsiteUrl: null,
            FacebookUrl: null,
            InstagramUrl: null,
            WhatsAppNumber: null
        );

        command.CacheKeys.Should().Contain("site-settings");
        command.CacheKeys.Should().Contain("Internal_SiteSettings");
    }
}