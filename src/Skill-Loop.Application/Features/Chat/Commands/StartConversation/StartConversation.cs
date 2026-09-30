using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat;

namespace Skill_Loop.Application.Features.Chat.Commands.StartConversation;

/// <summary>
/// ابدأ محادثة مع مستخدم تاني. لو المحادثة موجودة أصلاً بنرجّعها (Idempotent)
/// عشان الـ Frontend يقدر يندهها كل مرة يدوس فيها زرار "راسل".
/// </summary>
[AuthenticatedOnly]
public sealed record StartConversationCommand(Guid UserId, Guid OtherUserId) : ICommand<ConversationDto>;

public sealed class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.OtherUserId).NotEmpty().WithMessage("Other user ID is required.");
    }
}

public sealed class StartConversationCommandHandler : ICommandHandler<StartConversationCommand, ConversationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManagementService _userManagement;

    public StartConversationCommandHandler(IApplicationDbContext context, IUserManagementService userManagement)
    {
        _context = context;
        _userManagement = userManagement;
    }

    public async Task<Result<ConversationDto>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        // 1. مينفعش تكلم نفسك
        if (request.UserId == request.OtherUserId)
            return Result<ConversationDto>.Failure(ChatErrors.CannotChatWithSelf);

        // 2. الطرف التاني لازم يكون موجود ونشط
        var otherUserResult = await _userManagement.GetByIdAsync(request.OtherUserId, cancellationToken);
        if (otherUserResult.IsFailure)
            return Result<ConversationDto>.Failure(otherUserResult.Errors);

        var otherUser = otherUserResult.Data!;
        if (!otherUser.IsActive)
            return Result<ConversationDto>.Failure(ChatErrors.RecipientUnavailable);

        // 3. دوّر على محادثة موجودة (الترتيب ثابت زي ما في الـ Domain)
        var (one, two) = Conversation.NormalizePair(request.UserId, request.OtherUserId);

        var conversation = await _context.FirstOrDefaultAsync(
            _context.Conversations.Where(c => c.ParticipantOneId == one && c.ParticipantTwoId == two),
            cancellationToken);

        // 4. مفيش؟ اعمل واحدة جديدة
        if (conversation is null)
        {
            var createResult = Conversation.Create(request.UserId, request.OtherUserId);
            if (createResult.IsFailure)
                return Result<ConversationDto>.Failure(createResult.Errors);

            conversation = createResult.Data!;
            _context.Add(conversation);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 5. عدد الرسايل اللي مستنياني (لو المحادثة قديمة)
        var unreadCount = await _context.CountAsync(
            _context.ChatMessages.Where(m =>
                m.ConversationId == conversation.Id &&
                m.SenderId != request.UserId &&
                m.ReadAt == null),
            cancellationToken);

        return Result<ConversationDto>.Success(new ConversationDto(
            conversation.Id,
            otherUser.Id,
            otherUser.FullName,
            otherUser.AvatarUrl,
            conversation.LastMessagePreview,
            conversation.LastMessageAt,
            unreadCount));
    }
}
