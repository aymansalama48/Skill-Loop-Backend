using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Sessions; // مسار الجلسة
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

        // 2. Get session with its materials (ده الأب اللي بيدير كل حاجة)
        var session = await _dbContext.Sessions
            .Include(s => s.Materials)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(SessionMaterialErrors.NotFound); // أو SessionNotFound
        }

        // 3. Verify ownership
        if (session.InstructorId != currentUserId && session.OwnerId != currentUserId)
        {
            return Result.Failure(SessionMaterialErrors.NotOwner);
        }

        // 4. Find the specific material inside the session
        var material = session.Materials.FirstOrDefault(m => m.Id == request.MaterialId);

        if (material is null)
        {
            return Result.Failure(SessionMaterialErrors.NotFound);
        }

        // 5. Delete from Drive first
        var deleteResult = await _courseContentStorage.DeleteAsync(material.DriveFileId, cancellationToken);
        if (!deleteResult.IsSuccess)
        {
            return Result.Failure(SessionMaterialErrors.DeleteFailed);
        }

        // 6. Delete from Session Entity (هنا بنستخدم الدالة بتاعتك!)
        session.RemoveMaterial(material);

        // حفظ التغييرات على الداتابيز (EF Core هيمسح الـ Material لوحده لأننا مسحناه من الليستة)
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}