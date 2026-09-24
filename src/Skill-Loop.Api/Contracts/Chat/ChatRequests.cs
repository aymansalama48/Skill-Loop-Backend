using Skill_Loop.Api.Contracts.Common;

namespace Skill_Loop.Api.Contracts.Chat;

public sealed record StartConversationRequest(Guid OtherUserId);

public sealed record SendMessageRequest(string Content);

public sealed record GetConversationMessagesRequest : PaginationRequest;
