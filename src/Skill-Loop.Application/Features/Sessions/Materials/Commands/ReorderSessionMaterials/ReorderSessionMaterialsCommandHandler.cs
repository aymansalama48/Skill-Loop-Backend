using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Session;
using Microsoft.EntityFrameworkCore;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.ReorderSessionMaterials;

public sealed class ReorderSessionMaterialsCommandHandler : ICommandHandler<ReorderSessionMaterialsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ReorderSessionMaterialsCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ReorderSessionMaterialsCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate current user
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result.Failure(SessionMaterialErrors.NotOwner);
        }

        var currentUserId = _currentUser.UserId.Value;

        // 2. Get session and verify ownership
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(SessionMaterialErrors.SessionNotFound);
        }

        if (session.InstructorId != currentUserId && session.OwnerId != currentUserId)
        {
            return Result.Failure(SessionMaterialErrors.NotOwner);
        }

        // 3. Get all materials for this session
        var materials = await _dbContext.SessionMaterials
            .Where(m => m.SessionId == request.SessionId)
            .ToListAsync(cancellationToken);

        if (materials.Count == 0)
        {
            return Result.Success(); // Nothing to reorder
        }

        // 4. Validate that the provided IDs match exactly the session's materials
        var existingIds = materials.Select(m => m.Id).ToHashSet();
        var providedIds = request.OrderedMaterialIds.ToHashSet();

        if (!existingIds.SetEquals(providedIds))
        {
            return Result.Failure(SessionMaterialErrors.InvalidReorder);
        }

        // 5. Update sort order
        for (int i = 0; i < request.OrderedMaterialIds.Count; i++)
        {
            var materialId = request.OrderedMaterialIds[i];
            var material = materials.First(m => m.Id == materialId);
            material.SortOrder = i;
            _dbContext.Update(material);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}