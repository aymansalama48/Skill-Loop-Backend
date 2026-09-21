using MediatR;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}

// لو محتاج Handler لأمر مش بيرجع بيانات (ICommand اللي من غير TResponse)
public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}