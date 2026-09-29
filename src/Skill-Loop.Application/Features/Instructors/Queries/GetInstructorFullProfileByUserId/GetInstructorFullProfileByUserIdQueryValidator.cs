using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;

public sealed class GetInstructorFullProfileByUserIdQueryValidator : AbstractValidator<GetInstructorFullProfileByUserIdQuery>
{
    public GetInstructorFullProfileByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرّف المستخدم مطلوب.");
    }
}