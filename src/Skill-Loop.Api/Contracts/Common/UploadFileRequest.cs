using Microsoft.AspNetCore.Http;

namespace Skill_Loop.Api.Contracts.Common;

public sealed class UploadFileRequest
{
    public IFormFile File { get; set; } = null!;
}
