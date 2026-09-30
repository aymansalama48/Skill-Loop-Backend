using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Emails;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Emails.Commands.ResendEmail;

/// <summary>
/// Re-queues a previously logged email for delivery.
///
/// Security: resending must be restricted to non-transactional message types. A
/// PasswordReset log row contains a live OTP, so a blanket "resend by id" turns the
/// endpoint into a way to exfiltrate another user's reset code and to mail arbitrary
/// recipients. <see cref="ResendAllowedTypes"/> is the allowlist.
///
/// Merge note: origin/main both dropped the allowlist and moved the gate to
/// SiteSettings.Manage. The allowlist had to stay - <see cref="ResendEmailCommandHandler"/>
/// calls IsResendableType, so taking their side would not compile, and losing the check
/// would reintroduce the OTP-exfiltration path.
/// </summary>
[Permission(Permissions.Emails.Resend)]
public record ResendEmailCommand(Guid EmailLogId) : IRequest<Result<bool>>
{
    /// <summary>
    /// Only operator diagnostics may be replayed. Security-sensitive types (password reset,
    /// email verification) are deliberately excluded.
    /// </summary>
    public static readonly IReadOnlySet<string> ResendAllowedTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TestEmail"
        };

    public static bool IsResendableType(string? type) =>
        type is not null && ResendAllowedTypes.Contains(type);
}

public class ResendEmailCommandHandler : IRequestHandler<ResendEmailCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public ResendEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ResendEmailCommand request, CancellationToken cancellationToken)
    {
        var emailLog = await _context.EmailLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmailLogId, cancellationToken);

        if (emailLog == null)
            return Result<bool>.Failure("Email not found.");

        if (!ResendEmailCommand.IsResendableType(emailLog.Type))
            return Result<bool>.Failure(
                $"Emails of type '{emailLog.Type}' cannot be resent through this endpoint.");

        var newEmailLog = EmailLog.Create(
            type: emailLog.Type,
            recipientEmail: emailLog.RecipientEmail,
            referenceId: Guid.NewGuid().ToString(), // new ref so it runs again
            subject: emailLog.Subject,
            body: emailLog.Body
        );

        _context.Add(newEmailLog);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
