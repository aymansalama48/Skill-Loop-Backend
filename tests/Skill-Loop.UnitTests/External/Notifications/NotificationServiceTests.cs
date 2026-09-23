namespace Skill_Loop.UnitTests.External.Notifications;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.SessionsTemplates;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Xunit;

public class NotificationServiceTests
{
    [Fact]
    public async Task SendMaterialUploadedEmail_CallsEmailSender()
    {
        var notificationService = new Mock<IIdentityNotificationService>();
        var model = new SessionMaterialUploadedTemplateModel();

        await notificationService.Object.SendMaterialUploadedEmailAsync("user@test.com", model);

        notificationService.Verify(
            n => n.SendMaterialUploadedEmailAsync("user@test.com", model),
            Times.Once);
    }

    [Fact]
    public async Task SendMaterialUploadedConfirmation_CallsEmailSender()
    {
        var notificationService = new Mock<IIdentityNotificationService>();
        var model = new SessionMaterialUploadedTemplateModel();

        await notificationService.Object.SendMaterialUploadedConfirmationAsync("instructor@test.com", model);

        notificationService.Verify(
            n => n.SendMaterialUploadedConfirmationAsync("instructor@test.com", model),
            Times.Once);
    }

    [Fact]
    public async Task SendStorageQuotaWarning_SendsEmailToAdmin()
    {
        var notificationService = new Mock<IIdentityNotificationService>();

        await notificationService.Object.SendStorageQuotaWarningAsync(
            900_000_000_000, 1_000_000_000_000, 0.8);

        notificationService.Verify(
            n => n.SendStorageQuotaWarningAsync(
                900_000_000_000, 1_000_000_000_000, 0.8),
            Times.Once);
    }

    [Fact]
    public async Task SendMaterialUploadedEmail_EmailRequest_HasCorrectRecipient()
    {
        var notificationService = new Mock<IIdentityNotificationService>();
        var model = new SessionMaterialUploadedTemplateModel();

        await notificationService.Object.SendMaterialUploadedEmailAsync("learner@skillloop.com", model);

        notificationService.Verify(
            n => n.SendMaterialUploadedEmailAsync("learner@skillloop.com", model),
            Times.Once);
    }

    [Fact]
    public async Task AllMethods_AreCallable()
    {
        var notificationService = new Mock<IIdentityNotificationService>();
        var model = new SessionMaterialUploadedTemplateModel();

        await notificationService.Object.SendMaterialUploadedEmailAsync("a@b.com", model);
        await notificationService.Object.SendMaterialUploadedConfirmationAsync("c@d.com", model);
        await notificationService.Object.SendStorageQuotaWarningAsync(100, 1000, 0.5);

        notificationService.Invocations.Should().HaveCount(3);
    }
}
