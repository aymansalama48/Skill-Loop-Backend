using MediatR;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}