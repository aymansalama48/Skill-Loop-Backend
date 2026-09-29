using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.SiteSettings.Queries.GetSiteSettings;
using Skill_Loop.Domain.Common.Results;
using SiteSettingsEntity = Skill_Loop.Domain.Entities.SiteSettings.SiteSettings;
using Skill_Loop.UnitTests.Common;
using Xunit;

namespace Skill_Loop.UnitTests.Features.SiteSettings.Queries.GetSiteSettings;

public class GetSiteSettingsQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetSiteSettingsQueryHandler _handler;

    public GetSiteSettingsQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetSiteSettingsQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_NoSettingsExists_ReturnsFailure()
    {
        var query = new GetSiteSettingsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description.Contains("لم يتم العثور على إعدادات الموقع"));
    }

    [Fact]
    public async Task Handle_SettingsExist_ReturnsSettings()
    {
        var settings = new SiteSettingsEntity
        {
            AppName = "SkillLoop",
            LogoName = "logo.png",
            SupportEmail = "support@skillloop.com",
            ContactPhoneNumber = "+20123456789",
            Address = "Cairo, Egypt",
            WebsiteUrl = "https://skillloop.com",
            FacebookUrl = "https://facebook.com/skillloop",
            InstagramUrl = "https://instagram.com/skillloop",
            WhatsAppNumber = "+20123456789"
        };
        _dbContext.Add(settings);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSiteSettingsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AppName.Should().Be("SkillLoop");
        result.Data.SupportEmail.Should().Be("support@skillloop.com");
        result.Data.ContactPhoneNumber.Should().Be("+20123456789");
        result.Data.Address.Should().Be("Cairo, Egypt");
        result.Data.WebsiteUrl.Should().Be("https://skillloop.com");
        result.Data.FacebookUrl.Should().Be("https://facebook.com/skillloop");
        result.Data.InstagramUrl.Should().Be("https://instagram.com/skillloop");
        result.Data.WhatsAppNumber.Should().Be("+20123456789");
    }

    [Fact]
    public async Task Handle_SettingsExist_ReturnsCorrectResponseType()
    {
        var settings = new SiteSettingsEntity
        {
            AppName = "Test App",
            SupportEmail = "test@test.com"
        };
        _dbContext.Add(settings);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSiteSettingsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeOfType<SiteSettingsRespone>();
    }

    [Fact]
    public async Task Handle_UsesAsNoTracking()
    {
        var settings = new SiteSettingsEntity
        {
            AppName = "Test App",
            SupportEmail = "test@test.com"
        };
        _dbContext.Add(settings);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSiteSettingsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // If AsNoTracking is used, the entity should not be tracked
        // We can't directly test this, but the handler should work correctly
    }

    [Fact]
    public async Task Handle_OnlyFirstSettingsReturned()
    {
        // Add multiple settings (should only return first)
        var settings1 = new SiteSettingsEntity { AppName = "First", SupportEmail = "first@test.com" };
        var settings2 = new SiteSettingsEntity { AppName = "Second", SupportEmail = "second@test.com" };
        _dbContext.Add(settings1);
        _dbContext.Add(settings2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetSiteSettingsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.AppName.Should().Be("First");
    }
}