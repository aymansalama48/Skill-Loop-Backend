using MediatR;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Domain.Entities.Courses.Events;

namespace Skill_Loop.Application.Features.Courses.EventHandlers;

public sealed class CourseInvalidationHandler :
    INotificationHandler<DomainEventNotification<CourseCreatedDomainEvent>>,
    INotificationHandler<DomainEventNotification<CourseUpdatedDomainEvent>>,
    INotificationHandler<DomainEventNotification<CoursePublishedDomainEvent>>
{
    private readonly ICacheService _cacheService;

    public CourseInvalidationHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<CourseCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        await _cacheService.RemoveByPrefixAsync("courses:paged:", cancellationToken);
        await _cacheService.RemoveAsync("categories:all", cancellationToken);
    }

    public async Task Handle(DomainEventNotification<CourseUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        await _cacheService.RemoveAsync($"courses:detail:{notification.DomainEvent.CourseId}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("courses:paged:", cancellationToken);
    }

    public async Task Handle(DomainEventNotification<CoursePublishedDomainEvent> notification, CancellationToken cancellationToken)
    {
        await _cacheService.RemoveAsync($"courses:detail:{notification.DomainEvent.CourseId}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("courses:paged:", cancellationToken);
        await _cacheService.RemoveAsync("categories:all", cancellationToken);
    }
}
