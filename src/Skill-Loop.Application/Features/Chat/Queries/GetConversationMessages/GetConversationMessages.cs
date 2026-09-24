using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Chat;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Chat.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Chat.Queries.GetConversationMessages;

/// <summary>
/// رسايل محادثة معينة، الأحدث الأول (الصفحة 1 = آخر رسايل). الـ Frontend يقلبها للعرض.
/// </summary>
public sealed class GetConversationMessagesQuery : PaginationParameters, IQuery<PagedResult<MessageDto>>
{
    public Guid UserId { get; init; }
    public Guid ConversationId { get; init; }
}

public sealed class GetConversationMessagesQueryValidator : AbstractValidator<GetConversationMessagesQuery>
{
    public GetConversationMessagesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("Conversation ID is required.");
    }
}

public sealed class GetConversationMessagesQueryHandler : IQueryHandler<GetConversationMessagesQuery, PagedResult<MessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetConversationMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<MessageDto>>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _context.FirstOrDefaultAsync(
            _context.AsNoTracking(_context.Conversations.Where(c => c.Id == request.ConversationId)),
            cancellationToken);

        if (conversation is null)
            return Result<PagedResult<MessageDto>>.Failure(ChatErrors.ConversationNotFound);

        // أمان: ممنوع حد يقرا محادثة مش طرف فيها
        if (!conversation.HasParticipant(request.UserId))
            return Result<PagedResult<MessageDto>>.Failure(ChatErrors.NotParticipant);

        var baseQuery = _context.AsNoTracking(_context.ChatMessages)
            .Where(m => m.ConversationId == request.ConversationId);

        var totalCount = await _context.CountAsync(baseQuery, cancellationToken);

        var page = await _context.ToListAsync(
            baseQuery
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .Skip(request.Skip)
                .Take(request.PageSize),
            cancellationToken);

        return Result<PagedResult<MessageDto>>.Success(new PagedResult<MessageDto>
        {
            Items = page.Select(m => m.ToDto()).ToList(),
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        });
    }
}
