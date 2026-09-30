using Skill_Loop.Domain.Common.Errors.SupportQuestion;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Support;

public sealed class SupportQuestion : AuditableEntity
{
    public string Question { get; private set; } = string.Empty;
    public string? Answer { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }
    public string? UserEmail { get; private set; }
    public string? UserName { get; private set; }
    public Guid? AskedByUserId { get; private set; }
    public bool IsAnswered { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public bool EmailSent { get; private set; }
    public DateTime? EmailSentAt { get; private set; }

    private SupportQuestion() { }

    public static Result<SupportQuestion> Create(
        string question,
        string category,
        string? userEmail = null,
        string? userName = null,
        Guid? askedByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(question))
            return Result<SupportQuestion>.Failure(SupportQuestionErrors.EmptyQuestion);

        if (string.IsNullOrWhiteSpace(category))
            return Result<SupportQuestion>.Failure(SupportQuestionErrors.EmptyCategory);

        return Result<SupportQuestion>.Success(new SupportQuestion
        {
            Id = Guid.CreateVersion7(),
            Question = question.Trim(),
            Category = category.Trim(),
            UserEmail = userEmail?.Trim(),
            UserName = userName?.Trim(),
            AskedByUserId = askedByUserId,
            IsPublished = false,
            IsAnswered = false,
            EmailSent = false
        });
    }

    public static Result<SupportQuestion> CreatePublished(
        string question,
        string answer,
        string category)
    {
        if (string.IsNullOrWhiteSpace(question))
            return Result<SupportQuestion>.Failure(SupportQuestionErrors.EmptyQuestion);

        if (string.IsNullOrWhiteSpace(answer))
            return Result<SupportQuestion>.Failure(SupportQuestionErrors.EmptyAnswer);

        if (string.IsNullOrWhiteSpace(category))
            return Result<SupportQuestion>.Failure(SupportQuestionErrors.EmptyCategory);

        return Result<SupportQuestion>.Success(new SupportQuestion
        {
            Id = Guid.CreateVersion7(),
            Question = question.Trim(),
            Answer = answer.Trim(),
            Category = category.Trim(),
            IsPublished = true,
            IsAnswered = true,
            AnsweredAt = DateTime.UtcNow,
            EmailSent = false
        });
    }

    public Result SetAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return Result.Failure(SupportQuestionErrors.EmptyAnswer);

        Answer = answer.Trim();
        MarkAnswered(overwriteAnsweredAt: true);

        return Result.Success();
    }

    public Result Publish()
    {
        if (string.IsNullOrWhiteSpace(Answer))
            return Result.Failure(SupportQuestionErrors.NoAnswer);

        IsPublished = true;
        return Result.Success();
    }

    public Result Unpublish()
    {
        IsPublished = false;
        return Result.Success();
    }

    public Result MarkEmailSent()
    {
        EmailSent = true;
        EmailSentAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Update(string question, string? answer, string category, bool isPublished)
    {
        if (string.IsNullOrWhiteSpace(question))
            return Result.Failure(SupportQuestionErrors.EmptyQuestion);

        if (string.IsNullOrWhiteSpace(category))
            return Result.Failure(SupportQuestionErrors.EmptyCategory);

        Question = question.Trim();
        Answer = answer?.Trim();
        Category = category.Trim();
        IsPublished = isPublished;

        // التحديث بيسيب تاريخ أول إجابة زي ما هو — التعديلات اللاحقة مش بتنغيّره.
        if (!string.IsNullOrWhiteSpace(answer))
            MarkAnswered(overwriteAnsweredAt: false);

        return Result.Success();
    }

    /// <summary>
    /// القاعدة الواحدة اللي بتحكم حالة "تم الرد": المفتاح و وقت أول إجابة.
    /// <paramref name="overwriteAnsweredAt"/> بيحدد هل التعديل اللاحق يغيّر وقت الإجابة ولا لا.
    /// </summary>
    private void MarkAnswered(bool overwriteAnsweredAt)
    {
        IsAnswered = true;

        if (overwriteAnsweredAt || AnsweredAt is null)
            AnsweredAt = DateTime.UtcNow;
    }
}