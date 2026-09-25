using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorProfileByUserId;

public sealed class GetInstructorProfileByUserIdQueryValidator : AbstractValidator<GetInstructorProfileByUserIdQuery>
{
    public GetInstructorProfileByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرّف المستخدم مطلوب.");
    }
}