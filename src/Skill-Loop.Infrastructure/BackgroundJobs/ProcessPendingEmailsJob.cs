using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models;
using Skill_Loop.Domain.Entities.Emails;
using Skill_Loop.Infrastructure.Persistence.Data;

namespace Skill_Loop.Infrastructure.BackgroundJobs;

public class ProcessPendingEmailsJob
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ProcessPendingEmailsJob> _logger;
    private const int MaxRetryCount = 3;

    public ProcessPendingEmailsJob(
        AppDbContext dbContext,
        IEmailSender emailSender,
        ILogger<ProcessPendingEmailsJob> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task ProcessAsync()
    {
        var emails = await _dbContext.EmailLogs
            .Where(e => e.Status == EmailStatus.Pending && e.Attempts < MaxRetryCount)
            .OrderBy(e => e.CreatedAtUtc)
            .Take(50)
            .ToListAsync();

        if (!emails.Any()) return;

        foreach (var email in emails)
        {
            try
            {
                var request = new EmailRequest
                {
                    To = new List<string> { email.RecipientEmail },
                    Subject = email.Subject,
                    Body = email.Body,
                    IsHtml = true
                };

                await _emailSender.SendEmailAsync(request);

                email.MarkAsSent(DateTime.UtcNow);
                _logger.LogInformation("Email sent successfully to {RecipientEmail}, ID: {EmailId}", email.RecipientEmail, email.Id);
            }
            catch (Exception ex)
            {
                email.MarkAsFailed(ex.ToString());
                _logger.LogError(ex, "Failed to send email to {RecipientEmail}, ID: {EmailId}", email.RecipientEmail, email.Id);
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
