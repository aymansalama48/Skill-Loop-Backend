using Skill_Loop.Domain.Common.Errors.InstructorReview;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Instructors;

public sealed class InstructorReview : SoftDeleteEntity
{
    public Guid InstructorProfileId { get; private set; }
    public Guid LearnerUserId { get; private set; } // الطالب اللي عمل التقييم

    public int Rating { get; private set; } // التقييم من 1 لـ 5
    public string Comment { get; private set; } = string.Empty;

    private InstructorReview() { } // For EF Core

    public static Result<InstructorReview> Create(
        Guid instructorProfileId,
        Guid learnerUserId,
        int rating,
        string? comment)
    {
        if (rating < 1 || rating > 5)
            return Result<InstructorReview>.Failure(InstructorReviewErrors.InvalidRating);

        var review = new InstructorReview
        {
            InstructorProfileId = instructorProfileId,
            LearnerUserId = learnerUserId,
            Rating = rating,
            Comment = comment?.Trim() ?? string.Empty
        };

        return Result<InstructorReview>.Success(review);
    }
    public void Update(int rating, string? comment)
    {
        Rating = rating;
        Comment = comment?.Trim() ?? string.Empty;
    }
}