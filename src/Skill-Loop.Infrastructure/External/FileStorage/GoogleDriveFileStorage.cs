using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Infrastructure.External.FileStorage;

internal sealed class GoogleDriveFileStorage(ICourseContentStorage courseContentStorage) : IFileStorage
{
    private readonly ICourseContentStorage _courseContentStorage = courseContentStorage;

    public async Task<Result<string>> UploadAsync(Stream fileStream, string fileName, string folderName)
    {
        var uploadResult = await _courseContentStorage.UploadAsync(fileStream, fileName, folderName);
        if (uploadResult.IsFailure)
        {
            return Result<string>.Failure(uploadResult.Errors);
        }

        return Result<string>.Success(uploadResult.Data.DriveFileId);
    }

    public async Task<Result<bool>> ExistsAsync(string filePath)
    {
        return await Task.FromResult(Result<bool>.Success(true));
    }

    public async Task<Result> DeleteAsync(string filePath)
    {
        return await _courseContentStorage.DeleteAsync(filePath);
    }
}
