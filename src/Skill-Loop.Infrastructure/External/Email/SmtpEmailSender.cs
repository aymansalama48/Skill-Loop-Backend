using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.External.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly MailOptions _mailSettings;

    public SmtpEmailSender(IOptions<MailOptions> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendEmailAsync(EmailRequest request)
    {
        var message = new MimeMessage();

        var senderDisplayName = !string.IsNullOrWhiteSpace(request.SenderDisplayName)
            ? request.SenderDisplayName
            : _mailSettings.SenderName;

        message.From.Add(new MailboxAddress(senderDisplayName, _mailSettings.SenderEmail));

        if (request.To != null)
            message.To.AddRange(request.To.Select(MailboxAddress.Parse));

        if (request.Cc != null)
            message.Cc.AddRange(request.Cc.Select(MailboxAddress.Parse));

        if (request.Bcc != null)
            message.Bcc.AddRange(request.Bcc.Select(MailboxAddress.Parse));

        message.Subject = request.Subject;

        if (request.IsHtml)
        {
            var htmlPart = new TextPart("html") { Text = request.Body };
            htmlPart.ContentType.Charset = "utf-8";
            htmlPart.ContentTransferEncoding = ContentEncoding.QuotedPrintable;

            if (request.Attachments != null && request.Attachments.Any())
            {
                var multipart = new Multipart("mixed");
                multipart.Add(htmlPart);

                foreach (var attachment in request.Attachments)
                {
                    if (attachment.Content != null && attachment.Content != Stream.Null)
                    {
                        if (attachment.Content.CanSeek)
                            attachment.Content.Position = 0;

                        var mimePart = new MimePart(attachment.ContentType ?? "application/octet-stream")
                        {
                            Content = new MimeContent(attachment.Content),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = attachment.FileName
                        };
                        multipart.Add(mimePart);
                    }
                }
                message.Body = multipart;
            }
            else
            {
                message.Body = htmlPart;
            }
        }
        else
        {
            var textPart = new TextPart("plain") { Text = request.Body };
            textPart.ContentType.Charset = "utf-8";
            textPart.ContentTransferEncoding = ContentEncoding.QuotedPrintable;
            message.Body = textPart;
        }

        using var client = new SmtpClient();
        try
        {
            var secureOption = _mailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            if (_mailSettings.Port == 465) secureOption = SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, secureOption);

            if (!string.IsNullOrEmpty(_mailSettings.Username))
                await client.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);

            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}