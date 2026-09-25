using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Instructors.Share
{
    public sealed record InstructorSummaryResponse(
       Guid ProfileId,
       Guid UserId,
       string FullName,
       string? AvatarUrl,
       string Headline,
       double Rating,
       int SessionsCompleted
   );
}
