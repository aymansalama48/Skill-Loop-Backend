using Skill_Loop.Application.Common.Errors.SupportQuestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;

namespace Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;

public sealed class AnswerSupportQuestionCommandHandler : ICommandHandler<AnswerSupportQuestionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISupportAnswerNotifier _notificationService;
    private readonly ILogger<AnswerSupportQuestionCommandHandler> _logger;

    public AnswerSupportQuestionCommandHandler(
        IApplicationDbContext dbContext,
        ISupportAnswerNotifier notificationService,
        ILogger<AnswerSupportQuestionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AnswerSupportQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _dbContext.SupportQuestions
            .FirstOrDefaultAsync(sq => sq.Id == request.Id, cancellationToken);

        if (question is null)
        {
            return Result<Guid>.Failure(
                SupportQuestionErrors.NotFound);
        }

        var answerResult = question.SetAnswer(request.Answer);
        if (answerResult.IsFailure)
            return Result<Guid>.Failure(answerResult.Errors);

        if (request.Publish)
        {
            var publishResult = question.Publish();
            if (publishResult.IsFailure)
                return Result<Guid>.Failure(publishResult.Errors);
        }

        _dbContext.Update(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // الرد بيتبعت للمستخدم بإيميل. لو مفيش بريد محفوظ (سؤال handmade من الأدمن) مفيش إيميل يبعت.
        if (!string.IsNullOrWhiteSpace(question.UserEmail))
        {
            var emailResult = await _notificationService.SendAnswerNotificationAsync(
                question.UserEmail,
                question.UserName ?? question.UserEmail,
                question.Question,
                question.Answer!,
                question.Category,
                question.Id.ToString(),
                CancellationToken.None);

            if (emailResult.IsSuccess)
            {
                question.MarkEmailSent();
                _dbContext.Update(question);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // الرد اتحفظ بس الإيميل مابعتش — هنقدر نبعته تاني من غير ما نخسر الرد.
                _logger.LogError("Failed to send answer email for question {QuestionId}: {Errors}", question.Id, emailResult.Errors);
            }
        }

        return Result<Guid>.Success(question.Id);
    }
}
