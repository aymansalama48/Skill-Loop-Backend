using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;

public sealed class SendFaqAnswerEmailCommandHandler : ICommandHandler<SendFaqAnswerEmailCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISupportAnswerNotifier _notificationService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SendFaqAnswerEmailCommandHandler> _logger;

    public SendFaqAnswerEmailCommandHandler(
        IApplicationDbContext dbContext,
        ISupportAnswerNotifier notificationService,
        ICurrentUser currentUser,
        ILogger<SendFaqAnswerEmailCommandHandler> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(SendFaqAnswerEmailCommand request, CancellationToken cancellationToken)
    {
        var email = _currentUser.Email;
        if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure(
                new Error("SupportQuestion.Unauthenticated", "You must be logged in to request the answer by email.", ErrorType.Unauthorized));
        }

        var question = await _dbContext.SupportQuestions
            .FirstOrDefaultAsync(sq => sq.Id == request.Id, cancellationToken);

        if (question is null || !question.IsPublished || string.IsNullOrWhiteSpace(question.Answer))
        {
            return Result.Failure(
                new Error("SupportQuestion.AnswerNotAvailable", "لا توجد إجابة متاحة لهذا السؤال.", ErrorType.NotFound));
        }

        var result = await _notificationService.SendFaqAnswerAsync(
            email,
            _currentUser.FullName ?? email,
            question.Question,
            question.Answer,
            question.Id.ToString(),
            CancellationToken.None);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to send FAQ answer email for question {QuestionId}: {Errors}", question.Id, result.Errors);
            return Result.Failure(result.Errors);
        }

        return Result.Success();
    }
}
