using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses.Events;
using Skill_Loop.Domain.Entities.Courses.ValueObjects;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Courses;

public sealed class Course : SoftDeleteEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public CoursePrice Price { get; private set; } = CoursePrice.Free();
    public CourseLevel Level { get; private set; } = CourseLevel.AllLevels;
    public CourseStatus Status { get; private set; } = CourseStatus.Draft;
    public CourseRating Rating { get; private set; } = CourseRating.Empty();

    // Instructor Information
    public Guid InstructorId { get; private set; }
    public string InstructorName { get; private set; } = string.Empty;

    // Classification
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    // Aggregated Metrics
    public int TotalLessonsCount { get; private set; }
    public TimeSpan TotalDuration { get; private set; } = TimeSpan.Zero;

    // Navigation Collections
    private readonly List<Section> _sections = [];
    public IReadOnlyCollection<Section> Sections => _sections.AsReadOnly();

    private readonly List<CourseReview> _reviews = [];
    public IReadOnlyCollection<CourseReview> Reviews => _reviews.AsReadOnly();

    private readonly List<PdfAttachment> _attachments = [];
    public IReadOnlyCollection<PdfAttachment> Attachments => _attachments.AsReadOnly();

    private Course() { }

    public static Result<Course> Create(
        string title,
        string description,
        string thumbnailUrl,
        CoursePrice price,
        CourseLevel level,
        Guid instructorId,
        string instructorName,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<Course>.Failure(new Error("Course.TitleEmpty", "Title is required.", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(description))
            return Result<Course>.Failure(new Error("Course.DescriptionEmpty", "Description is required.", ErrorType.Validation));

        if (instructorId == Guid.Empty)
            return Result<Course>.Failure(new Error("Course.InvalidInstructor", "Instructor is required.", ErrorType.Validation));

        if (categoryId == Guid.Empty)
            return Result<Course>.Failure(new Error("Course.InvalidCategory", "Category is required.", ErrorType.Validation));

        var course = new Course
        {
            Id = Guid.CreateVersion7(),
            Title = title.Trim(),
            Description = description.Trim(),
            ThumbnailUrl = thumbnailUrl.Trim(),
            Price = price,
            Level = level,
            Status = CourseStatus.Draft,
            InstructorId = instructorId,
            InstructorName = instructorName.Trim(),
            CategoryId = categoryId,
            Rating = CourseRating.Empty(),
            TotalDuration = TimeSpan.Zero,
            TotalLessonsCount = 0
        };

        course.AddDomainEvent(new CourseCreatedDomainEvent(course.Id, course.Title));
        return Result<Course>.Success(course);
    }

    public Result UpdateDetails(
        string title,
        string description,
        string thumbnailUrl,
        CoursePrice price,
        CourseLevel level,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(new Error("Course.TitleEmpty", "Title is required.", ErrorType.Validation));

        Title = title.Trim();
        Description = description.Trim();
        ThumbnailUrl = thumbnailUrl.Trim();
        Price = price;
        Level = level;
        CategoryId = categoryId;

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result AddSection(string title, int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(new Error("Section.EmptyTitle", "Section title is required.", ErrorType.Validation));

        var section = Section.Create(Id, title.Trim(), orderIndex);
        _sections.Add(section);

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result AddLessonToSection(Guid sectionId, string title, VideoResource video, int orderIndex, bool isPreviewable = false)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(new Error("Section.NotFound", "Target section does not exist in this course.", ErrorType.NotFound));

        var lesson = Lesson.Create(sectionId, title, video, orderIndex, isPreviewable);
        section.AddLesson(lesson);

        TotalLessonsCount++;
        TotalDuration += video.Duration;

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result AddAttachment(PdfAttachment attachment)
    {
        _attachments.Add(attachment);
        return Result.Success();
    }

    public Result AddReview(Guid userId, int stars, string? comment)
    {
        if (stars is < 1 or > 5)
            return Result.Failure(new Error("Review.InvalidStars", "Rating must be between 1 and 5.", ErrorType.Validation));

        var review = CourseReview.Create(Id, userId, stars, comment);
        _reviews.Add(review);
        Rating = Rating.AddRating(stars);

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public void Publish()
    {
        if (Status != CourseStatus.Published)
        {
            Status = CourseStatus.Published;
            AddDomainEvent(new CoursePublishedDomainEvent(Id, Title));
        }
    }

    public void Archive()
    {
        Status = CourseStatus.Archived;
    }
}
