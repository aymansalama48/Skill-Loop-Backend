using MediatR;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Emails;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Emails.Commands.TestEmail;

[AuthenticatedOnly]
public record TestEmailCommand(string To, string Subject) : IRequest<Result<string>>;

public class TestEmailCommandHandler : IRequestHandler<TestEmailCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public TestEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(TestEmailCommand request, CancellationToken cancellationToken)
    {
        var emailLog = EmailLog.Create(
            type: "TestEmail",
            recipientEmail: request.To,
            referenceId: Guid.NewGuid().ToString(), // Unique id to avoid idempotency conflict
            subject: request.Subject,
            body: "<p>This is a test email sent from the Admin dashboard.</p>"
        );

        _context.Add(emailLog);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(emailLog.Id.ToString());
    }
}
