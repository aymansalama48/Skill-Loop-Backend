using Skill_Loop.Domain.Common.Errors.Review;
using Skill_Loop.Domain.Common.Errors.Lesson;
using Skill_Loop.Domain.Common.Errors.Section;
using Skill_Loop.Domain.Common.Errors.Course;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses.Events;

using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Courses;

public sealed class Course : SoftDeleteEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public int Credits { get; private set; }
    public bool IsFree => Credits == 0;
    public CourseLevel Level { get; private set; } = CourseLevel.AllLevels;
    public CourseStatus Status { get; private set; } = CourseStatus.Draft;
    public double AverageRating { get; private set; }
    public int TotalReviews { get; private set; }

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

    private readonly List<CourseMaterial> _attachments = [];
    public IReadOnlyCollection<CourseMaterial> Attachments => _attachments.AsReadOnly();

    private Course() { }

    public static Result<Course> Create(
        string title,
        string description,
        string thumbnailUrl,
        int credits,
        CourseLevel level,
        Guid instructorId,
        string instructorName,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<Course>.Failure(CourseErrors.TitleEmpty);

        if (string.IsNullOrWhiteSpace(description))
            return Result<Course>.Failure(CourseErrors.DescriptionEmpty);

        if (instructorId == Guid.Empty)
            return Result<Course>.Failure(CourseErrors.InvalidInstructor);

        if (categoryId == Guid.Empty)
            return Result<Course>.Failure(CourseErrors.InvalidCategory);

        if (credits < 0)
            return Result<Course>.Failure(CourseErrors.NegativeCredits);

        var course = new Course
        {
            Id = Guid.CreateVersion7(),
            Title = title.Trim(),
            Description = description.Trim(),
            ThumbnailUrl = thumbnailUrl?.Trim() ?? string.Empty,
            Credits = credits,
            Level = level,
            Status = CourseStatus.Draft,
            InstructorId = instructorId,
            InstructorName = instructorName?.Trim() ?? string.Empty,
            CategoryId = categoryId,
            AverageRating = 0.0,
            TotalReviews = 0,
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
        int credits,
        CourseLevel level,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(CourseErrors.TitleEmpty);

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(CourseErrors.DescriptionEmpty);

        if (credits < 0)
            return Result.Failure(CourseErrors.NegativeCredits);

        if (categoryId == Guid.Empty)
            return Result.Failure(CourseErrors.InvalidCategory);

        Title = title.Trim();
        Description = description.Trim();
        ThumbnailUrl = thumbnailUrl?.Trim() ?? string.Empty;
        Credits = credits;
        Level = level;
        CategoryId = categoryId;

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result AddSection(string title, int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(SectionErrors.EmptyTitle);

        var section = Section.Create(Id, title.Trim(), orderIndex);
        _sections.Add(section);

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result AddLessonToSection(Guid sectionId, string title, int orderIndex, bool isPreviewable = false)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        var lesson = Lesson.Create(sectionId, title, orderIndex, isPreviewable);
        section.AddLesson(lesson);

        TotalLessonsCount++;

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result UpdateSection(Guid sectionId, string title, int orderIndex)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(SectionErrors.EmptyTitle);

        section.UpdateDetails(title, orderIndex);
        
        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result RemoveSection(Guid sectionId)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        // Decrease totals
        foreach (var lesson in section.Lessons)
        {
            TotalLessonsCount--;
            TotalDuration -= lesson.Duration;
        }

        _sections.Remove(section);
        
        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result ReorderSections(Dictionary<Guid, int> sectionOrders)
    {
        foreach (var section in _sections)
        {
            if (sectionOrders.TryGetValue(section.Id, out int newOrderIndex))
            {
                section.UpdateOrder(newOrderIndex);
            }
        }
        
        _sections.Sort((a, b) => a.OrderIndex.CompareTo(b.OrderIndex));
        
        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result UpdateLesson(Guid sectionId, Guid lessonId, string title, int orderIndex, bool isPreviewable)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        var lesson = section.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson is null)
            return Result.Failure(LessonErrors.NotFound);

        lesson.UpdateDetails(title, orderIndex, isPreviewable);
        
        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result UpdateLessonVideo(Guid sectionId, Guid lessonId, string videoUrl, TimeSpan duration, string? streamingResolution, string? externalProviderId)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        var lesson = section.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson is null)
            return Result.Failure(LessonErrors.NotFound);

        if (string.IsNullOrWhiteSpace(videoUrl))
            return Result.Failure(LessonErrors.EmptyVideoUrl);

        if (duration <= TimeSpan.Zero)
            return Result.Failure(LessonErrors.InvalidDuration);

        // Update totals
        TotalDuration = TotalDuration - lesson.Duration + duration;

        lesson.UpdateVideo(videoUrl, duration, streamingResolution, externalProviderId);
        
        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result RemoveLesson(Guid sectionId, Guid lessonId)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        var lesson = section.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson is null)
            return Result.Failure(LessonErrors.NotFound);

        section.RemoveLesson(lesson);
        
        TotalLessonsCount--;
        TotalDuration -= lesson.Duration;

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result ReorderLessons(Guid sectionId, Dictionary<Guid, int> lessonOrders)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
            return Result.Failure(SectionErrors.NotFound);

        foreach (var lesson in section.Lessons)
        {
            if (lessonOrders.TryGetValue(lesson.Id, out int newOrderIndex))
            {
                lesson.UpdateOrder(newOrderIndex);
            }
        }
        
        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result AddAttachment(CourseMaterial attachment)
    {
        _attachments.Add(attachment);
        return Result.Success();
    }

    public Result AddReview(Guid userId, int stars, string? comment)
    {
        if (stars is < 1 or > 5)
            return Result.Failure(ReviewErrors.InvalidStars);

        if (userId == InstructorId)
            return Result.Failure(ReviewErrors.InstructorCannotReview);

        if (_reviews.Any(r => r.UserId == userId))
            return Result.Failure(ReviewErrors.Duplicate);

        var review = CourseReview.Create(Id, userId, stars, comment);
        _reviews.Add(review);
        
        var totalScore = (AverageRating * TotalReviews) + stars;
        TotalReviews++;
        AverageRating = Math.Round(totalScore / TotalReviews, 2);

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result UpdateReview(Guid userId, int stars, string? comment)
    {
        if (stars is < 1 or > 5)
            return Result.Failure(ReviewErrors.InvalidStars);

        var review = _reviews.FirstOrDefault(r => r.UserId == userId);
        if (review is null)
            return Result.Failure(ReviewErrors.NotFound);

        var totalScore = (AverageRating * TotalReviews) - review.Stars + stars;
        AverageRating = Math.Round(totalScore / TotalReviews, 2);

        review.Update(stars, comment);

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result RemoveReview(Guid userId)
    {
        var review = _reviews.FirstOrDefault(r => r.UserId == userId);
        if (review is null)
            return Result.Failure(ReviewErrors.NotFound);

        _reviews.Remove(review);

        TotalReviews--;
        if (TotalReviews == 0)
        {
            AverageRating = 0;
        }
        else
        {
            var totalScore = (AverageRating * (TotalReviews + 1)) - review.Stars;
            AverageRating = Math.Round(totalScore / TotalReviews, 2);
        }

        AddDomainEvent(new CourseUpdatedDomainEvent(Id, Title));
        return Result.Success();
    }

    public Result Publish()
    {
        if (string.IsNullOrWhiteSpace(ThumbnailUrl))
            return Result.Failure(CourseErrors.MissingThumbnail);

        if (_sections.Count == 0)
            return Result.Failure(CourseErrors.NoSections);

        if (_sections.Any(s => s.Lessons.Count == 0))
            return Result.Failure(CourseErrors.EmptySections);

        if (Status != CourseStatus.Published)
        {
            Status = CourseStatus.Published;
            AddDomainEvent(new CoursePublishedDomainEvent(Id, Title));
        }
        
        return Result.Success();
    }

    public void Archive()
    {
        Status = CourseStatus.Archived;
    }
}
