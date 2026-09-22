using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Courses.ValueObjects;

public sealed record CoursePrice
{
    public int Credits { get; private init; }
    public bool IsFree => Credits == 0;

    private CoursePrice(int credits)
    {
        Credits = credits;
    }

    public static Result<CoursePrice> Create(int credits)
    {
        if (credits < 0)
        {
            return Result<CoursePrice>.Failure(new Error("CoursePrice.Negative", "Course credits cannot be negative.", ErrorType.Validation));
        }

        return Result<CoursePrice>.Success(new CoursePrice(credits));
    }

    public static CoursePrice Free() => new(0);
}
