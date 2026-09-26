using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Instructors.Share
{
    public sealed record InstructorProfileResponse(
    Guid Id,
    Guid UserId,
    string Headline,
    string Bio,
    bool IsApproved,
    double Rating,
    int SessionsCompleted,
    int CreditsEarned
);
}
