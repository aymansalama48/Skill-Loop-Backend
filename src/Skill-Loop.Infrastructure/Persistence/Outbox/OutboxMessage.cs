namespace Skill_Loop.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }                          // رقم الرسالة
    public string Type { get; set; } = default!;         // نوع الخبر (اسم الـ Event)
    public string Content { get; set; } = default!;       // بيانات الخبر (JSON)
    public DateTime OccurredOnUtc { get; set; }            // إمتى اتسجل
    public DateTime? ProcessedOnUtc { get; set; }          // إمتى اتبعت (null = لسه)
    public string? Error { get; set; }                     // لو حصل خطأ
    public int RetryCount { get; set; }                    // عدد المحاولات
}