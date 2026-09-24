using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.External.Realtime;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Chat.Commands.MarkConversationRead;

/// <summary>
/// علّم كل الرسايل اللي جتلي في المحادثة دي إنها اتقرت.
/// </summary>
public sealed record MarkConversationReadCommand(Guid UserId, Guid ConversationId) : ICommand;

public sealed class MarkConversationReadCommandValidator : AbstractValidator<MarkConversationReadCommand>
{
    public MarkConversationReadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("Conversation ID is required.");
    }
}

public sealed class MarkConversationReadCommandHandler : ICommandHandler<MarkConversationReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IChatNotifier _notifier;

    public MarkConversationReadCommandHandler(IApplicationDbContext context, IChatNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.FirstOrDefaultAsync(
            _context.AsNoTracking(_context.Conversations.Where(c => c.Id == request.ConversationId)),
            cancellationToken);

        if (conversation is null)
            return Result.Failure(ChatErrors.ConversationNotFound);

        if (!conversation.HasParticipant(request.UserId))
            return Result.Failure(ChatErrors.NotParticipant);

        // الرسايل اللي الطرف التاني بعتها وأنا لسه ما قريتهاش
        var unread = await _context.ToListAsync(
            _context.ChatMessages.Where(m =>
                m.ConversationId == request.ConversationId &&
                m.SenderId != request.UserId &&
                m.ReadAt == null),
            cancellationToken);

        if (unread.Count == 0)
            return Result.Success();

        foreach (var message in unread)
            message.MarkAsRead();

        await _context.SaveChangesAsync(cancellationToken);

        // قول للمُرسِل إن رسايله اتقرت (علامة "✓✓")
        var readAt = unread.Max(m => m.ReadAt!.Value);
        await _notifier.NotifyMessagesReadAsync(
            conversation.GetOtherParticipantId(request.UserId),
            new MessagesReadDto(conversation.Id, request.UserId, readAt),
            cancellationToken);

        return Result.Success();
    }
}
