using Skill_Loop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Sessions.Queries.Share
{
    public sealed record SessionResponse(
        Guid Id,
        Guid InstructorId,
        Guid OwnerId,
        string Title,
        SessionStatus Status,
        DateTime CreatedAt);
}
