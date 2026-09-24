using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.External.Realtime;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat;

namespace Skill_Loop.Application.Features.Chat.Commands.SendMessage;

/// <summary>
/// ابعت رسالة في محادثة. الـ Command ده بيتنادى من مكانين: الـ REST Controller والـ SignalR Hub
/// وبيشتغلوا بنفس المنطق بالظبط (مكان واحد للـ Business Logic).
/// </summary>
public sealed record SendMessageCommand(Guid SenderId, Guid ConversationId, string Content) : ICommand<MessageDto>;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("Sender ID is required.");
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("Conversation ID is required.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("لا يمكن إرسال رسالة فارغة.")
            .MaximumLength(ChatMessage.MaxContentLength)
            .WithMessage($"الرسالة أطول من الحد المسموح ({ChatMessage.MaxContentLength} حرف).");
    }
}

public sealed class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, MessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IChatNotifier _notifier;

    public SendMessageCommandHandler(IApplicationDbContext context, IChatNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // 1. هات المحادثة (Tracked لأننا هنعدّل آخر رسالة فيها)
        var conversation = await _context.FirstOrDefaultAsync(
            _context.Conversations.Where(c => c.Id == request.ConversationId),
            cancellationToken);

        if (conversation is null)
            return Result<MessageDto>.Failure(ChatErrors.ConversationNotFound);

        // 2. أمان: لازم يكون المُرسِل طرف في المحادثة دي
        if (!conversation.HasParticipant(request.SenderId))
            return Result<MessageDto>.Failure(ChatErrors.NotParticipant);

        // 3. اعمل الرسالة (الـ Domain بيتأكد إنها مش فاضية ومش طويلة)
        var messageResult = ChatMessage.Create(conversation.Id, request.SenderId, request.Content);
        if (messageResult.IsFailure)
            return Result<MessageDto>.Failure(messageResult.Errors);

        var message = messageResult.Data!;

        // 4. حدّث "آخر رسالة" في المحادثة + ضيف الرسالة، واحفظ الاتنين مع بعض
        conversation.RegisterMessage(message);
        _context.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. بعد الحفظ بنجاح: ابعت للطرف التاني لحظياً
        var dto = message.ToDto();
        var recipientId = conversation.GetOtherParticipantId(request.SenderId);
        await _notifier.NotifyMessageReceivedAsync(recipientId, dto, cancellationToken);

        return Result<MessageDto>.Success(dto);
    }
}
