using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Notifications;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : ICommand;

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public sealed class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkNotificationReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        // بنفلتر بالـ UserId كمان (مش بس الـ Id) عشان محدش يقدر يعلّم إشعار شخص تاني كمقروء
        var notification = await _context.FirstOrDefaultAsync(
            _context.Notifications.Where(n => n.Id == request.NotificationId && n.UserId == request.UserId),
            cancellationToken);

        if (notification is null)
            return Result.Failure(NotificationErrors.NotFound);

        notification.MarkAsRead();
        _context.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
