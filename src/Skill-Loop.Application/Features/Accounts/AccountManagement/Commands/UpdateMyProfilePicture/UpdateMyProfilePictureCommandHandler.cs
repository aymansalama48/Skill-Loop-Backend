using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;

public sealed class UpdateMyProfilePictureCommandHandler : ICommandHandler<UpdateMyProfilePictureCommand, string>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserManagementService _userService;
    private readonly IFileStorage _fileStorage;

    public UpdateMyProfilePictureCommandHandler(
        ICurrentUser currentUser,
        IUserManagementService userService,
        IFileStorage fileStorage)
    {
        _currentUser = currentUser;
        _userService = userService;
        _fileStorage = fileStorage;
    }

    public async Task<Result<string>> Handle(UpdateMyProfilePictureCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result<string>.Failure(UserErrors.NotFound);
        }

        // 1. رفع الصورة عبر خدمة التخزين
        var uploadResult = await _fileStorage.UploadAsync(
            request.FileStream,
            request.FileName,
            folderName: "avatars");

        if (uploadResult.IsFailure)
        {
            return Result<string>.Failure(uploadResult.Errors);
        }

        var avatarUrl = uploadResult.Data!;

        // 2. تحديث الرابط في جدول المستخدمين
        var updateResult = await _userService.UpdateProfilePictureAsync(
            _currentUser.UserId.Value,
            avatarUrl,
            cancellationToken);

        if (!updateResult.IsSuccess)
        {
            // اختياري: حذف الملف في حال فشل التحديث في الداتابيز
            await _fileStorage.DeleteAsync(avatarUrl);
            return Result<string>.Failure(updateResult.Errors);
        }

        return Result<string>.Success(avatarUrl);
    }
}