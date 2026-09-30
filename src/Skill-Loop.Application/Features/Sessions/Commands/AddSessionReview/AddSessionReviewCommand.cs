using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Sessions.Commands.AddSessionReview;

public sealed record AddSessionReviewCommand(
    Guid UserId,
    Guid SessionId,
    int Stars,
    string? Comment) : ICommand;
