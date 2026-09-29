using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;

namespace Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;

public sealed class SubmitContactFormCommandHandler : ICommandHandler<SubmitContactFormCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISupportRequestNotifier _notificationService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SubmitContactFormCommandHandler> _logger;

    public SubmitContactFormCommandHandler(
        IApplicationDbContext dbContext,
        ISupportRequestNotifier notificationService,
        ICurrentUser currentUser,
        ILogger<SubmitContactFormCommandHandler> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(SubmitContactFormCommand request, CancellationToken cancellationToken)
    {
        var email = _currentUser.Email;
        if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(email))
        {
            return Result<Guid>.Failure(
                new Error("SupportQuestion.Unauthenticated", "You must be logged in to submit a question.", ErrorType.Unauthorized));
        }

        var userName = string.IsNullOrWhiteSpace(_currentUser.FullName) ? email : _currentUser.FullName;

        var createResult = SupportQuestion.Create(
            question: $"[{request.Subject}] {request.Message}",
            category: request.Category,
            userEmail: email,
            userName: userName,
            askedByUserId: _currentUser.UserId);

        if (createResult.IsFailure)
            return Result<Guid>.Failure(createResult.Errors);

        var question = createResult.Data!;

        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var reference = question.Id.ToString()[..8].ToUpper();

        // الإيميلات ما بتخليش الـ Request يفشل — لو فشلت بنسجلها وبنكمّل.
        // بنستخدم CancellationToken.None عشان لو العميل قفل الصفحة mids-request الإيميل يفضل يتبعت.
        var confirmation = await _notificationService.SendContactFormConfirmationAsync(
            email, userName, request.Subject, reference, CancellationToken.None);

        if (confirmation.IsFailure)
            _logger.LogWarning("Failed to send support confirmation email for question {QuestionId}: {Errors}", question.Id, confirmation.Errors);

        var teamNotification = await _notificationService.SendSupportTeamNotificationAsync(
            userName, email, request.Subject, request.Message, request.Category, question.Id.ToString(), CancellationToken.None);

        if (teamNotification.IsFailure)
            _logger.LogError("Failed to notify the support team about new question {QuestionId}: {Errors}", question.Id, teamNotification.Errors);

        return Result<Guid>.Success(question.Id);
    }
}
