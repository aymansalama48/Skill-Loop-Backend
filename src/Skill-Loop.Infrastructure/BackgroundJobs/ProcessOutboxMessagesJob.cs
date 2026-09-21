using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Domain.Common.Events;
using Skill_Loop.Infrastructure.Persistence.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
namespace Skill_Loop.Infrastructure.BackgroundJobs;

public class ProcessOutboxMessagesJob
{
    private readonly AppDbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly IDateTime _dateTime;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;
    private const int MaxRetryCount = 3;

    public ProcessOutboxMessagesJob(
        AppDbContext dbContext,
        IPublisher publisher,
        IDateTime dateTime,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _dateTime = dateTime;
        _logger = logger;
    }

    // دالة المعالجة التي سينفذها Hangfire
    [AutomaticRetry(Attempts = 0)]
    public async Task ProcessAsync()
    {
        // 1. سحب الرسائل المعلقة
        var messages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < MaxRetryCount)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync();

        if (!messages.Any()) return;

        foreach (var message in messages)
        {
            try
            {
                // 2. فك التشفير للحدث الأصلي
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    _logger.LogError("تعذر التعرف على نوع الحدث: {Type}", message.Type);
                    message.Error = $"Type {message.Type} missing.";
                    message.RetryCount = MaxRetryCount;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
                if (domainEvent is null)
                {
                    _logger.LogError("فشل فك تشفير محتوى الحدث للرسالة ذات المعرف: {Id}", message.Id);
                    message.Error = "Failed to deserialize event content.";
                    message.RetryCount = MaxRetryCount;
                    continue;
                }

                // 3. نشر الحدث داخل التطبيق عبر MediatR (💡 هنا التعديل الجوهري)

                // أ. إنشاء النوع المغلف ديناميكياً: DomainEventNotification<TEvent>
                var wrapperType = typeof(DomainEventNotification<>).MakeGenericType(eventType);

                // ب. إنشاء نسخة من المغلف وتمرير الحدث الأصلي بداخلها
                var notification = Activator.CreateInstance(wrapperType, domainEvent);

                // ج. إرسال المغلف (الذي يطبق INotification) إلى MediatR
                if (notification is not null)
                {
                    await _publisher.Publish(notification);
                }

                // 4. تحديث حالة الرسالة
                message.ProcessedOnUtc = _dateTime.Now;
                message.Error = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء معالجة Outbox Message ID: {Id}", message.Id);
                message.RetryCount++;
                message.Error = ex.ToString();
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
