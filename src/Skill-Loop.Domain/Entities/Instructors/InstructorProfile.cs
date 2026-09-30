using Skill_Loop.Domain.Common.Errors.InstructorReview;
using Skill_Loop.Domain.Common.Errors.InstructorProfile;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking;

namespace Skill_Loop.Domain.Entities.Instructors;

public sealed class InstructorProfile : AuditableEntity
{
    public Guid UserId { get; private set; }

    // بيانات العرض
    public string Headline { get; private set; } = string.Empty;
    public string Bio { get; private set; } = string.Empty;

    // حالة الاعتماد (ممكن مستقبلاً الإدارة توافق عليه الأول)
    public bool IsApproved { get; private set; }

    // الإحصائيات 
    public double Rating { get; private set; }
    public int SessionsCompleted { get; private set; }
    public int CreditsEarned { get; private set; }

    // المراجعات والتقييمات
    private readonly List<InstructorReview> _reviews = new();
    public IReadOnlyCollection<InstructorReview> Reviews => _reviews.AsReadOnly();
    // ==========================================
    // إدارة مواعيد العمل المتاحة
    // ==========================================
    private readonly List<InstructorAvailability> _availabilities = new();
    public IReadOnlyCollection<InstructorAvailability> Availabilities => _availabilities.AsReadOnly();

    private InstructorProfile() { } // For EF Core

    public static Result<InstructorProfile> Create(Guid userId, string headline, string bio)
    {
        if (userId == Guid.Empty)
            return Result<InstructorProfile>.Failure(InstructorProfileErrors.InvalidUser);

        var profile = new InstructorProfile
        {
            UserId = userId,
            Headline = headline,
            Bio = bio,
            IsApproved = false, // يحتاج موافقة كوضع افتراضي
            Rating = 0.0,
            SessionsCompleted = 0,
            CreditsEarned = 0
        };

        return Result<InstructorProfile>.Success(profile);
    }

    public Result UpdateDetails(string headline, string bio)
    {
        Headline = headline;
        Bio = bio;
        return Result.Success();
    }

    public void Approve() => IsApproved = true;
    public void Suspend() => IsApproved = false;

    // دوال لتحديث الإحصائيات (الرصيد والجلسات)
    public void IncrementSessionsCompleted() => SessionsCompleted++;
    public void AddCreditsEarned(int amount) => CreditsEarned += amount;

    // دالة إضافة التقييم وتحديث المتوسط تلقائياً
    public Result AddReview(Guid learnerUserId, int rating, string? comment)
    {
        if (_reviews.Any(r => r.LearnerUserId == learnerUserId))
        {
            return Result.Failure(InstructorProfileErrors.AlreadyReviewed);
        }

        var reviewResult = InstructorReview.Create(Id, learnerUserId, rating, comment);
        if (!reviewResult.IsSuccess)
            return reviewResult;

        _reviews.Add(reviewResult.Data);

        // حساب المتوسط الجديد للتقييم بمجرد إضافة المراجعة
        RecalculateRating();

        return Result.Success();
    }
    public Result UpdateReview(Guid reviewId, Guid learnerUserId, int rating, string? comment)
    {
        var review = _reviews.FirstOrDefault(r => r.Id == reviewId);

        if (review is null)
        {
            return Result.Failure(InstructorReviewErrors.NotFound);
        }

        if (review.LearnerUserId != learnerUserId)
        {
            return Result.Failure(InstructorReviewErrors.Unauthorized);
        }

        if (rating < 1 || rating > 5)
        {
            return Result.Failure(InstructorReviewErrors.InvalidRating);
        }

        review.Update(rating, comment);

        // إعادة حساب التقييم بعد التعديل
        RecalculateRating();

        return Result.Success();
    }

    public Result RemoveReview(Guid reviewId, Guid learnerUserId)
    {
        var review = _reviews.FirstOrDefault(r => r.Id == reviewId);

        if (review is null)
        {
            return Result.Failure(InstructorReviewErrors.NotFound);
        }

        if (review.LearnerUserId != learnerUserId)
        {
            return Result.Failure(InstructorReviewErrors.Unauthorized);
        }

        _reviews.Remove(review);

        // إعادة حساب التقييم بعد الحذف
        RecalculateRating();

        return Result.Success();
    }
    /// <summary>
    /// إضافة موعد جديد متاح للعمل
    /// </summary>
    public Result AddAvailability(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        // التحقق من عدم وجود تداخل في المواعيد لنفس اليوم (اختياري ولكنه مفيد)
        var hasOverlap = _availabilities.Any(a =>
            a.DayOfWeek == dayOfWeek &&
            (startTime < a.EndTime && endTime > a.StartTime));

        if (hasOverlap)
        {
            return Result.Failure(InstructorProfileErrors.AvailabilityOverlap);
        }

        var availabilityResult = InstructorAvailability.Create(Id, dayOfWeek, startTime, endTime);
        if (!availabilityResult.IsSuccess)
            return availabilityResult;

        _availabilities.Add(availabilityResult.Data);
        return Result.Success();
    }

    /// <summary>
    /// حذف موعد متاح
    /// </summary>
    public Result RemoveAvailability(Guid availabilityId)
    {
        var availability = _availabilities.FirstOrDefault(a => a.Id == availabilityId);

        if (availability is null)
        {
            return Result.Failure(InstructorProfileErrors.AvailabilityNotFound);
        }

        _availabilities.Remove(availability);
        return Result.Success();
    }

    // دالة خاصة (Private) لحساب متوسط التقييم من الليستة الفعلية
    private void RecalculateRating()
    {
        if (_reviews.Count == 0)
        {
            Rating = 0.0;
            return;
        }

        double average = _reviews.Average(r => r.Rating);
        Rating = Math.Round(average, 2); // تقريب لرقمين عشريين
    }
}