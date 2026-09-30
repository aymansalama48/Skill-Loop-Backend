using Skill_Loop.Application.Common.Errors.InstructorReview;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;

public sealed class AddInstructorReviewCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<AddInstructorReviewCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddInstructorReviewCommand request, CancellationToken cancellationToken)
    {
        // بنجيب البروفايل مع التقييمات السابقة عشان يقدر يحسب المتوسط
        var profile = await _dbContext.InstructorProfiles
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == request.InstructorProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<Guid>.Failure(InstructorProfileErrors.NotFound);
        }

        // لو عاوز تمنع المدرب يقيم نفسه
        if (profile.UserId == request.LearnerUserId)
        {
            return Result<Guid>.Failure(InstructorReviewErrors.CannotReviewSelf);
        }

        // إضافة التقييم عبر الـ Domain Logic
        var reviewResult = profile.AddReview(request.LearnerUserId, request.Rating, request.Comment);
        if (!reviewResult.IsSuccess)
        {
            return Result<Guid>.Failure(reviewResult.Errors);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // بما إننا مبنرجعش الـ Id بتاع الـ Review في الدالة، ممكن نرجعه عن طريق الفلترة بأحدث واحد للطالب ده
        var newReviewId = profile.Reviews.First(r => r.LearnerUserId == request.LearnerUserId).Id;

        return Result<Guid>.Success(newReviewId);
    }
}