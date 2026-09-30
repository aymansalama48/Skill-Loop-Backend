using Skill_Loop.Api.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;


namespace Skill_Loop.Api.Controllers.Test;

[Authorize(Roles = "Admin,SuperAdmin")]
[Route("api/test/files")]
public class TestFilesController : BaseApiController
{
    private readonly IFileStorage _fileStorage;

    public TestFilesController(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// تجربة رفع ملف
    /// </summary>
    [HttpPost("upload")]
    public async Task<IResult> Upload(
        IFormFile file,
        [FromQuery] string folderName = "test-uploads")
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest("الرجاء اختيار ملف صالح للرفع.");
        }

        await using var stream = file.OpenReadStream();

        var result = await _fileStorage.UploadAsync(
            stream,
            file.FileName,
            folderName);

        return HandleResult(result);
    }

    /// <summary>
    /// تجربة التحقق من وجود ملف
    /// </summary>
    [HttpGet("exists")]
    public async Task<IResult> Exists([FromQuery] string filePath)
    {
        var result = await _fileStorage.ExistsAsync(filePath);
        return HandleResult(result);
    }

    /// <summary>
    /// تجربة حذف ملف
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IResult> Delete([FromQuery] string filePath)
    {
        var result = await _fileStorage.DeleteAsync(filePath);
        return HandleResult(result);
    }
}