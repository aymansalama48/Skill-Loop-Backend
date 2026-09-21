using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand;