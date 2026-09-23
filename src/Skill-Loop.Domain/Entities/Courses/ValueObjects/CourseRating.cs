namespace Skill_Loop.Domain.Entities.Courses.ValueObjects;

public sealed record CourseRating
{
    public double AverageRating { get; private init; }
    public int TotalReviews { get; private init; }

    public CourseRating() : this(0.0, 0) { }

    private CourseRating(double averageRating, int totalReviews)
    {
        AverageRating = averageRating;
        TotalReviews = totalReviews;
    }

    public static CourseRating Empty() => new(0.0, 0);

    public static CourseRating Create(double averageRating, int totalReviews) =>
        new(Math.Round(averageRating, 2), Math.Max(0, totalReviews));

    public CourseRating AddRating(int newScore)
    {
        var totalScore = (AverageRating * TotalReviews) + newScore;
        var newCount = TotalReviews + 1;
        return new CourseRating(Math.Round(totalScore / newCount, 2), newCount);
    }
}
