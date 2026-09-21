using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Errors.Files;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.External.FileStorage;

internal sealed class LocalFileStorage(IOptions<FileStorageOptions> options) : IFileStorage
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<Result<string>> UploadAsync(
        Stream fileStream,
        string fileName,
        string folderName)
    {
        // ===========================================
        // Validation
        // ===========================================

        if (fileStream is null || fileStream.Length == 0)
            return Result<string>.Failure(FileErrors.EmptyFile);

        if (string.IsNullOrWhiteSpace(fileName))
            return Result<string>.Failure(FileErrors.InvalidFileName);

        if (string.IsNullOrWhiteSpace(folderName))
            return Result<string>.Failure(FileErrors.InvalidFolder);

        var maxBytes = _options.MaxFileSizeInMB * 1024 * 1024;

        if (fileStream.Length > maxBytes)
            return Result<string>.Failure(FileErrors.FileTooLarge);

        var extension = Path.GetExtension(fileName);

        if (_options.AllowedExtensions.Any() &&
            !_options.AllowedExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
        {
            return Result<string>.Failure(FileErrors.UnsupportedExtension);
        }

        // ===========================================
        // Build Paths
        // ===========================================

        var uploadsRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            _options.RootFolder);

        var targetFolder = Path.Combine(
            uploadsRoot,
            folderName);

        if (!Directory.Exists(targetFolder))
        {
            if (_options.CreateIfNotExists)
            {
                Directory.CreateDirectory(targetFolder);
            }
            else
            {
                return Result<string>.Failure(FileErrors.InvalidFolder);
            }
        }

        // ===========================================
        // File Name
        // ===========================================

        string finalFileName;

        if (_options.GenerateUniqueFileName)
        {
            if (_options.PreserveOriginalFileName)
            {
                var originalName = Path.GetFileNameWithoutExtension(fileName);

                finalFileName = $"{originalName}_{Guid.NewGuid():N}{extension}";
            }
            else
            {
                finalFileName = $"{Guid.NewGuid():N}{extension}";
            }
        }
        else
        {
            finalFileName = Path.GetFileName(fileName);
        }

        var fullPath = Path.Combine(
            targetFolder,
            finalFileName);

        // ===========================================
        // Existing File
        // ===========================================

        if (File.Exists(fullPath) && !_options.OverwriteExistingFiles)
        {
            return Result<string>.Failure(FileErrors.FileAlreadyExists);
        }

        // ===========================================
        // Save
        // ===========================================

        using (var output = new FileStream(
            fullPath,
            _options.OverwriteExistingFiles ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true))
        {
            if (fileStream.CanSeek)
                fileStream.Position = 0;

            await fileStream.CopyToAsync(output);
        }

        var relativePath = Path.Combine(_options.RootFolder, folderName, finalFileName)
            .Replace("\\", "/");

        return Result<string>.Success(relativePath);
    }

    public Task<Result<bool>> ExistsAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(Result<bool>.Success(false));

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            filePath);

        return Task.FromResult(Result<bool>.Success(File.Exists(fullPath)));
    }

    public Task<Result> DeleteAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(Result.Failure(FileErrors.FileNotFound));

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            filePath);

        if (!File.Exists(fullPath))
            return Task.FromResult(Result.Failure(FileErrors.FileNotFound));

        File.Delete(fullPath);

        return Task.FromResult(Result.Success());
    }
}