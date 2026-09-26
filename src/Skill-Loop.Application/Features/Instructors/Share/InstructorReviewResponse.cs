using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Instructors.Share
{

    public sealed record InstructorReviewResponse(
        Guid Id,
        Guid LearnerUserId, // ID الطالب اللي عمل التقييم
        int Rating,
        string Comment,
        DateTime CreatedOnUtc // عشان نعرض تاريخ التقييم (موجودة في الـ AuditableEntity)
    );
}
