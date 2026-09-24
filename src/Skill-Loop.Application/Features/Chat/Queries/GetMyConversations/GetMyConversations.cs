using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Chat.Queries.GetMyConversations;

/// <summary>
/// قايمة محادثاتي (الأحدث فوق) مع اسم الطرف التاني وعدد الرسايل غير المقروءة.
/// </summary>
public sealed record GetMyConversationsQuery(Guid UserId) : IQuery<IReadOnlyList<ConversationDto>>;

public sealed class GetMyConversationsQueryHandler : IQueryHandler<GetMyConversationsQuery, IReadOnlyList<ConversationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManagementService _userManagement;

    public GetMyConversationsQueryHandler(IApplicationDbContext context, IUserManagementService userManagement)
    {
        _context = context;
        _userManagement = userManagement;
    }

    public async Task<Result<IReadOnlyList<ConversationDto>>> Handle(GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        // 1. محادثاتي أنا (سواء كنت الطرف الأول أو التاني)
        var conversations = await _context.ToListAsync(
            _context.AsNoTracking(_context.Conversations)
                .Where(c => c.ParticipantOneId == request.UserId || c.ParticipantTwoId == request.UserId)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt),
            cancellationToken);

        if (conversations.Count == 0)
            return Result<IReadOnlyList<ConversationDto>>.Success(new List<ConversationDto>());

        // 2. عدد الرسايل غير المقروءة لكل محادثة (في Query واحدة مش واحدة لكل محادثة)
        var conversationIds = conversations.Select(c => c.Id).ToList();

        var unreadRows = await _context.ToListAsync(
            _context.ChatMessages
                .Where(m => conversationIds.Contains(m.ConversationId)
                            && m.SenderId != request.UserId
                            && m.ReadAt == null)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() }),
            cancellationToken);

        var unreadByConversation = unreadRows.ToDictionary(x => x.ConversationId, x => x.Count);

        // 3. أسماء وصور الأطراف التانية (Query واحدة برضه)
        var otherIds = conversations
            .Select(c => c.GetOtherParticipantId(request.UserId))
            .Distinct()
            .ToList();

        var users = await _userManagement.GetUsersByIdsAsync(otherIds, cancellationToken);
        var usersById = users.ToDictionary(u => u.Id);

        // 4. جمّع كل حاجة في DTO
        var result = conversations.Select(c =>
        {
            var otherId = c.GetOtherParticipantId(request.UserId);
            usersById.TryGetValue(otherId, out var other);

            return new ConversationDto(
                c.Id,
                otherId,
                other?.FullName ?? "مستخدم غير معروف",
                other?.AvatarUrl,
                c.LastMessagePreview,
                c.LastMessageAt,
                unreadByConversation.GetValueOrDefault(c.Id));
        }).ToList();

        return Result<IReadOnlyList<ConversationDto>>.Success(result);
    }
}
