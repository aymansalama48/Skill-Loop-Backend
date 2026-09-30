using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Emails;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Emails.Commands.ResendEmail;

[Permission(Permissions.SiteSettings.Manage)]
public record ResendEmailCommand(Guid EmailLogId) : IRequest<Result<bool>>;

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
            .FirstOrDefaultAsync(e => e.Id == request.EmailLogId, cancellationToken);

        if (emailLog == null)
            return Result<bool>.Failure("Email not found");

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
