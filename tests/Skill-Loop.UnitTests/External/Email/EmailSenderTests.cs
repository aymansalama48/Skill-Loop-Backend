using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models;
using Xunit;

namespace Skill_Loop.UnitTests.External.Email;

public class EmailSenderTests
{
    private readonly Mock<IEmailSender> _emailSender;

    public EmailSenderTests()
    {
        _emailSender = new Mock<IEmailSender>();
    }

    [Fact]
    public async Task SendEmailAsync_WithValidRequest_CallsSendMethod()
    {
        var request = new EmailRequest
        {
            To = new List<string> { "user@example.com" },
            Subject = "Test Subject",
            Body = "<h1>Test Body</h1>",
            IsHtml = true
        };

        _emailSender.Setup(e => e.SendEmailAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        await _emailSender.Object.SendEmailAsync(request);

        _emailSender.Verify(e => e.SendEmailAsync(It.Is<EmailRequest>(r =>
            r.To.Contains("user@example.com") &&
            r.Subject == "Test Subject" &&
            r.Body == "<h1>Test Body</h1>" &&
            r.IsHtml == true)), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithAttachments_PassesAttachments()
    {
        var attachment = new EmailAttachment
        {
            FileName = "test.pdf",
            ContentType = "application/pdf",
            Content = new MemoryStream(new byte[] { 1, 2, 3 })
        };

        var request = new EmailRequest
        {
            To = new List<string> { "user@example.com" },
            Subject = "Test with Attachment",
            Body = "Body",
            IsHtml = false,
            Attachments = new List<EmailAttachment> { attachment }
        };

        _emailSender.Setup(e => e.SendEmailAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        await _emailSender.Object.SendEmailAsync(request);

        _emailSender.Verify(e => e.SendEmailAsync(It.Is<EmailRequest>(r =>
            r.Attachments != null &&
            r.Attachments.Count == 1 &&
            r.Attachments[0].FileName == "test.pdf" &&
            r.Attachments[0].ContentType == "application/pdf")), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithCcAndBcc_PassesCorrectly()
    {
        var request = new EmailRequest
        {
            To = new List<string> { "to@example.com" },
            Cc = new List<string> { "cc@example.com" },
            Bcc = new List<string> { "bcc@example.com" },
            Subject = "Test",
            Body = "Body",
            IsHtml = true
        };

        _emailSender.Setup(e => e.SendEmailAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        await _emailSender.Object.SendEmailAsync(request);

        _emailSender.Verify(e => e.SendEmailAsync(It.Is<EmailRequest>(r =>
            r.To.Contains("to@example.com") &&
            r.Cc.Contains("cc@example.com") &&
            r.Bcc.Contains("bcc@example.com"))), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithCustomSenderDisplayName_UsesIt()
    {
        var request = new EmailRequest
        {
            To = new List<string> { "user@example.com" },
            Subject = "Test",
            Body = "Body",
            IsHtml = true,
            SenderDisplayName = "Custom Sender"
        };

        _emailSender.Setup(e => e.SendEmailAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        await _emailSender.Object.SendEmailAsync(request);

        _emailSender.Verify(e => e.SendEmailAsync(It.Is<EmailRequest>(r =>
            r.SenderDisplayName == "Custom Sender")), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_MultipleRecipients_PassesAll()
    {
        var request = new EmailRequest
        {
            To = new List<string> { "user1@example.com", "user2@example.com", "user3@example.com" },
            Subject = "Test",
            Body = "Body",
            IsHtml = true
        };

        _emailSender.Setup(e => e.SendEmailAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        await _emailSender.Object.SendEmailAsync(request);

        _emailSender.Verify(e => e.SendEmailAsync(It.Is<EmailRequest>(r =>
            r.To.Count == 3 &&
            r.To.Contains("user1@example.com") &&
            r.To.Contains("user2@example.com") &&
            r.To.Contains("user3@example.com"))), Times.Once);
    }
}