namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.Logout;

using Skill_Loop.Application.Common.Abstractions.Messaging;


public sealed record LogoutCommand(
    string RefreshToken) : ICommand<bool>;