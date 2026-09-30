using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Commands.AddSessionReview;

[AuthenticatedOnly]
public sealed record AddSessionReviewCommand(
    Guid UserId,
    Guid SessionId,
    int Stars,
    string? Comment) : ICommand;
