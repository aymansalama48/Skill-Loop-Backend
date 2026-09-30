using FluentValidation;
using MediatR;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.Settings;
using Skill_Loop.Domain.Entities.Emails;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Emails.Commands.TestEmail;

/// <summary>
/// Queues a diagnostic email to verify SMTP connectivity.
///
/// Security: this writes to the platform's own outbound relay. If the recipient is
/// attacker-controlled, the API becomes an open spam relay that sends mail as the
/// platform's domain and burns its sending reputation - which in turn breaks every
/// transactional message (password resets, booking confirmations). The recipient is
/// therefore restricted to addresses the platform itself owns; see
/// <see cref="TestEmailCommandHandler"/>.
///
/// Merge note: origin/main grouped this under SiteSettings.Manage, which is unrelated to
/// email. Kept as Emails.SendTest.
/// </summary>
[Permission(Permissions.Emails.SendTest)]
public record TestEmailCommand(string To, string Subject) : IRequest<Result<string>>;

public class TestEmailCommandHandler(
    IApplicationDbContext context,
    ISiteSettingsService siteSettingsService) : IRequestHandler<TestEmailCommand, Result<string>>
{
    private const string TestEmailType = "TestEmail";
    private const string TestEmailBody = "<p>This is a test email sent from the Admin dashboard.</p>";

    public async Task<Result<string>> Handle(TestEmailCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve the platform's own addresses. The request-supplied recipient is only
        //    honoured when it matches one of these.
        var settings = await siteSettingsService.GetSettingsAsync(cancellationToken);

        var allowed = new List<string?>();
        if (!string.IsNullOrWhiteSpace(settings.SupportEmail))
            allowed.Add(settings.SupportEmail.Trim());

        var requested = request.To?.Trim();
        var isAllowed = allowed.Any(a =>
            string.Equals(a, requested, StringComparison.OrdinalIgnoreCase));

        if (!isAllowed)
        {
            return Result<string>.Failure(
                new Error(
                    "Emails.TestRecipientNotAllowed",
                    "Test emails may only be sent to the platform's own support address. " +
                    "Sending to arbitrary recipients would expose the API as an open mail relay.",
                    ErrorType.Validation));
        }

        // 2. Queue for delivery by ProcessPendingEmailsJob.
        var emailLog = EmailLog.Create(
            type: TestEmailType,
            recipientEmail: requested!,
            referenceId: Guid.NewGuid().ToString(), // Unique id to avoid idempotency conflict
            subject: request.Subject,
            body: TestEmailBody
        );

        context.Add(emailLog);
        await context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(emailLog.Id.ToString());
    }
}

internal sealed class TestEmailCommandValidator : AbstractValidator<TestEmailCommand>
{
    public TestEmailCommandValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("This field is required.")
            .EmailAddress().WithMessage("Invalid value.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(200).WithMessage("Too long.");
    }
}
