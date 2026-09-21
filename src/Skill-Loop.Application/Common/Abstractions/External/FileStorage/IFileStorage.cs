using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.External.FileStorage;

public interface IFileStorage
{
    /// <summary>
    /// رفع ملف وإرجاع المسار النسبي له.
    /// </summary>
    Task<Result<string>> UploadAsync(
        Stream fileStream,
        string fileName,
        string folderName);

    /// <summary>
    /// التحقق من وجود الملف.
    /// </summary>
    Task<Result<bool>> ExistsAsync(
        string filePath);

    /// <summary>
    /// حذف ملف.
    /// </summary>
    Task<Result> DeleteAsync(
        string filePath);
}
