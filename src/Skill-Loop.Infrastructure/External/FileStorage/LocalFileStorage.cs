using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Errors.Files;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.External.FileStorage;

internal sealed class LocalFileStorage(
    IOptions<FileStorageOptions> options,
    IOptions<BaseUrlOptions> baseUrlOptions) : IFileStorage
{
    private readonly FileStorageOptions _options = options.Value;
    private readonly BaseUrlOptions _baseUrlOptions = baseUrlOptions.Value;

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

        if (!TryResolveInStorage(folderName, out var targetFolder))
            return Result<string>.Failure(FileErrors.InvalidFolder);

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

        var baseUrl = _baseUrlOptions.Backend.TrimEnd('/');
        var fullUrl = $"{baseUrl}/{relativePath}";

        return Result<string>.Success(fullUrl);
    }

    public Task<Result<bool>> ExistsAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(Result<bool>.Success(false));

        if (!TryResolveInStorage(filePath, out var fullPath))
            return Task.FromResult(Result<bool>.Success(false));

        return Task.FromResult(Result<bool>.Success(File.Exists(fullPath)));
    }

    public Task<Result> DeleteAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(Result.Failure(FileErrors.FileNotFound));

        if (!TryResolveInStorage(filePath, out var fullPath))
            return Task.FromResult(Result.Failure(FileErrors.FileNotFound));

        if (!File.Exists(fullPath))
            return Task.FromResult(Result.Failure(FileErrors.FileNotFound));

        File.Delete(fullPath);

        return Task.FromResult(Result.Success());
    }

    private string StorageRoot =>
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), _options.RootFolder));

    /// <summary>
    /// Resolves a caller-supplied relative path to an absolute path that is guaranteed to sit
    /// inside <see cref="StorageRoot"/>. Accepts both root-relative paths ("categories/icon.png")
    /// and paths already carrying the root folder name ("UploadedFiles/categories/icon.png"),
    /// which is the form <see cref="UploadAsync"/> returns. Anything that escapes via "..",
    /// a rooted path, or an invalid path character is rejected.
    /// </summary>
    private bool TryResolveInStorage(string relativePath, out string fullPath)
    {
        fullPath = string.Empty;

        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        if (relativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return false;

        var currentDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());
        var root = StorageRoot;
        var candidates = new List<string>();

        if (Path.IsPathRooted(relativePath))
        {
            // Absolute path handed back by UploadAsync when RootFolder itself is absolute.
            candidates.Add(relativePath);
        }
        else
        {
            var segments = relativePath.Split(
                new[] { '/', '\\' },
                StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0 || segments.Any(s => s == "." || s == ".."))
                return false;

            var relative = Path.Combine(segments);

            // UploadAsync hands back a path that already carries the root folder name
            // ("UploadedFiles/probe-dir/a.txt"). Prepending the root again produced
            // ".../UploadedFiles/UploadedFiles/probe-dir/a.txt", which is still inside the
            // root, so the containment check below accepted it and returned it first. The
            // real file was then reported as missing by ExistsAsync and DeleteAsync, so a
            // value returned by UploadAsync could not be passed back to either of them.
            var alreadyRooted = string.Equals(
                segments[0], _options.RootFolder.Trim('/', '\\'),
                StringComparison.OrdinalIgnoreCase);

            if (!alreadyRooted)
                candidates.Add(Path.Combine(root, relative));

            candidates.Add(Path.Combine(currentDirectory, relative));
        }

        foreach (var candidate in candidates)
        {
            string resolved;
            try
            {
                resolved = Path.GetFullPath(candidate);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                continue;
            }

            if (resolved.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                fullPath = resolved;
                return true;
            }
        }

        return false;
    }
}