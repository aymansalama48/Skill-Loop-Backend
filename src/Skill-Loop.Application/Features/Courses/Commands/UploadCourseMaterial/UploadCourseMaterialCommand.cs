using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Domain.Enums;
using System;
using System.IO;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.UploadCourseMaterial;

[AuthenticatedOnly]
public sealed record UploadCourseMaterialCommand(
    Guid CourseId,
    Stream FileStream,
    string FileName,
    string MimeType,
    long SizeBytes,
    MaterialType? MaterialType) : ICommand<Guid>, ICourseCommand;
