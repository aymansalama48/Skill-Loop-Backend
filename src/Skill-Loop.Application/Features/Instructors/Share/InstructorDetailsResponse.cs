using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Instructors.Share
{
    public sealed record InstructorDetailsResponse(
        Guid ProfileId,
        Guid UserId,
        string FirstName,
        string LastName,
        string FullName,
        string? AvatarUrl,
        string Headline,
        string Bio,
        bool IsApproved,
        double Rating,
        int SessionsCompleted,
        int CreditsEarned
    );
}
