using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Domain.Enums;
using System;
using System.IO;

namespace Skill_Loop.Application.Features.Courses.Commands.UploadCourseMaterial;

public sealed record UploadCourseMaterialCommand(
    Guid CourseId,
    Stream FileStream,
    string FileName,
    string MimeType,
    long SizeBytes,
    MaterialType? MaterialType) : ICommand<Guid>, ICourseCommand;
