using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.SessionMaterial;
using Microsoft.EntityFrameworkCore;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.DeleteSessionMaterial;

public sealed class DeleteSessionMaterialCommandHandler : ICommandHandler<DeleteSessionMaterialCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICourseContentStorage _courseContentStorage;
    private readonly ICurrentUser _currentUser;

    public DeleteSessionMaterialCommandHandler(
        IApplicationDbContext dbContext,
        ICourseContentStorage courseContentStorage,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _courseContentStorage = courseContentStorage;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteSessionMaterialCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate current user
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result.Failure(SessionMaterialErrors.NotOwner);
        }

        var currentUserId = _currentUser.UserId.Value;

        // 2. Get material with session
        var material = await _dbContext.SessionMaterials
            .Include(m => m.Session)
            .FirstOrDefaultAsync(m => m.Id == request.MaterialId && m.SessionId == request.SessionId, cancellationToken);

        if (material is null)
        {
            return Result.Failure(SessionMaterialErrors.NotFound);
        }

        // 3. Verify ownership (InstructorId or OwnerId)
        if (material.Session.InstructorId != currentUserId && material.Session.OwnerId != currentUserId)
        {
            return Result.Failure(SessionMaterialErrors.NotOwner);
        }

        // 4. Delete from Drive first
        var deleteResult = await _courseContentStorage.DeleteAsync(material.DriveFileId, cancellationToken);
        if (!deleteResult.IsSuccess)
        {
            return Result.Failure(SessionMaterialErrors.DeleteFailed);
        }

        // 5. Delete from DB
        _dbContext.Remove(material);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}