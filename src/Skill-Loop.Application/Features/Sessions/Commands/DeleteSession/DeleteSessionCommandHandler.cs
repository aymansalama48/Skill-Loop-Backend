using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Sessions.Commands.DeleteSession;

public sealed class DeleteSessionCommandHandler : ICommandHandler<DeleteSessionCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteSessionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .Include(s => s.Materials)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (session is null)
        {
            return Result.Failure(new Error("Session.NotFound", "الجلسة غير موجودة.", ErrorType.NotFound));
        }

        // حماية لمنع مسح جلسة بها ملفات حتى لا تظل الملفات معلقة في Google Drive
        if (session.Materials.Any())
        {
            return Result.Failure(new Error("Session.HasMaterials", "لا يمكن حذف جلسة تحتوي على ملفات. يرجى حذف الملفات أولاً.", ErrorType.Conflict));
        }

        _dbContext.Remove(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}