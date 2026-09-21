using MediatR;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
