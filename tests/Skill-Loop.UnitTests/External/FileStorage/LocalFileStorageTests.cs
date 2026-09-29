using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Errors.Files;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.External.FileStorage;
using Skill_Loop.Infrastructure.Options;
using Xunit;

namespace Skill_Loop.UnitTests.External.FileStorage;

public class LocalFileStorageTests
{
    private readonly string _testRoot;
    private readonly LocalFileStorage _storage;
    private readonly IOptions<FileStorageOptions> _options;

    public LocalFileStorageTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "SkillLoop_FileStorage_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testRoot);

        _options = Options.Create(new FileStorageOptions
        {
            RootFolder = _testRoot,
            MaxFileSizeInMB = 10,
            AllowedExtensions = new List<string> { ".txt", ".pdf", ".png", ".jpg", ".jpeg" },
            GenerateUniqueFileName = true,
            PreserveOriginalFileName = true,
            CreateIfNotExists = true,
            OverwriteExistingFiles = false
        });

        _storage = new LocalFileStorage(_options);
    }

    [Fact]
    public async Task UploadAsync_WithEmptyStream_ReturnsEmptyFileError()
    {
        var stream = new MemoryStream();

        var result = await _storage.UploadAsync(stream, "test.txt", "uploads");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.EmptyFile.Code);
    }

    [Fact]
    public async Task UploadAsync_WithNullStream_ReturnsEmptyFileError()
    {
        var result = await _storage.UploadAsync(null!, "test.txt", "uploads");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.EmptyFile.Code);
    }

    [Fact]
    public async Task UploadAsync_WithEmptyFileName_ReturnsInvalidFileNameError()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await _storage.UploadAsync(stream, "", "uploads");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.InvalidFileName.Code);
    }

    [Fact]
    public async Task UploadAsync_WithEmptyFolder_ReturnsInvalidFolderError()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await _storage.UploadAsync(stream, "test.txt", "");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.InvalidFolder.Code);
    }

    [Fact]
    public async Task UploadAsync_WithFileTooLarge_ReturnsFileTooLargeError()
    {
        var largeStream = new MemoryStream(new byte[11 * 1024 * 1024]); // 11MB > 10MB limit

        var result = await _storage.UploadAsync(largeStream, "large.txt", "uploads");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.FileTooLarge.Code);
    }

    [Fact]
    public async Task UploadAsync_WithDisallowedExtension_ReturnsUnsupportedExtensionError()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await _storage.UploadAsync(stream, "test.exe", "uploads");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.UnsupportedExtension.Code);
    }

    [Fact]
    public async Task UploadAsync_WithValidFile_ReturnsSuccessAndRelativePath()
    {
        var content = "Hello, World!";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var result = await _storage.UploadAsync(stream, "hello.txt", "uploads");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain("uploads/");
        result.Data.Should().EndWith(".txt");

        var fullPath = Path.Combine(_testRoot, result.Data!.Replace("/", Path.DirectorySeparatorChar.ToString()));
        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadAsync_WithValidFile_ContentIsPreserved()
    {
        var content = "Test content for upload";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var result = await _storage.UploadAsync(stream, "test.txt", "uploads");

        var fullPath = Path.Combine(_testRoot, result.Data!.Replace("/", Path.DirectorySeparatorChar.ToString()));
        var readContent = await File.ReadAllTextAsync(fullPath);
        readContent.Should().Be(content);
    }

    [Fact]
    public async Task UploadAsync_GeneratesUniqueFileName_WhenEnabled()
    {
        var stream1 = new MemoryStream(new byte[] { 1, 2, 3 });
        var stream2 = new MemoryStream(new byte[] { 4, 5, 6 });

        var result1 = await _storage.UploadAsync(stream1, "test.txt", "uploads");
        var result2 = await _storage.UploadAsync(stream2, "test.txt", "uploads");

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Data.Should().NotBe(result2.Data);
    }

    [Fact]
    public async Task UploadAsync_PreservesOriginalName_WhenEnabled()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await _storage.UploadAsync(stream, "my-document.pdf", "uploads");

        result.Data.Should().Contain("my-document_");
    }

    [Fact]
    public async Task ExistsAsync_WithExistingFile_ReturnsTrue()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var uploadResult = await _storage.UploadAsync(stream, "test.txt", "uploads");

        var result = await _storage.ExistsAsync(uploadResult.Data!);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingFile_ReturnsFalse()
    {
        var result = await _storage.ExistsAsync("uploads/nonexistent.txt");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyPath_ReturnsFalse()
    {
        var result = await _storage.ExistsAsync("");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingFile_ReturnsSuccess()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var uploadResult = await _storage.UploadAsync(stream, "test.txt", "uploads");

        var result = await _storage.DeleteAsync(uploadResult.Data!);

        result.IsSuccess.Should().BeTrue();

        var fullPath = Path.Combine(_testRoot, uploadResult.Data!.Replace("/", Path.DirectorySeparatorChar.ToString()));
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingFile_ReturnsFileNotFoundError()
    {
        var result = await _storage.DeleteAsync("uploads/nonexistent.txt");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.FileNotFound.Code);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyPath_ReturnsFileNotFoundError()
    {
        var result = await _storage.DeleteAsync("");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == FileErrors.FileNotFound.Code);
    }

    [Fact]
    public async Task UploadAsync_CreatesFolderIfNotExists()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var newFolder = "brand-new-folder";

        var result = await _storage.UploadAsync(stream, "test.txt", newFolder);

        result.IsSuccess.Should().BeTrue();
        Directory.Exists(Path.Combine(_testRoot, newFolder)).Should().BeTrue();
    }
}