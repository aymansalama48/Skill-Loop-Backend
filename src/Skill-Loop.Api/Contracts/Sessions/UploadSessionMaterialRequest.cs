using Microsoft.AspNetCore.Http;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Contracts.Sessions;

public sealed class UploadSessionMaterialRequest
{
    public IFormFile File { get; set; } = default!;

    // اختياري: لو حابب تحدد نوع الملف من الواجهة
    public MaterialType? MaterialType { get; set; }
}