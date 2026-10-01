using Skill_Loop.Application.Common.Abstractions.Messaging;
using System.IO;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using System;

namespace Skill_Loop.Application.Features.Courses.Commands.UploadCourseThumbnail;

[Permission(Permissions.Courses.Update)]
public sealed record UploadCourseThumbnailCommand(
    Guid CourseId,
    Stream ThumbnailStream,
    string ThumbnailFileName) : ICommand<string>;
