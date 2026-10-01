using Skill_Loop.Application.Common.Abstractions.Messaging;
using System.IO;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using System;
using Skill_Loop.Application.Features.Courses.Common;

namespace Skill_Loop.Application.Features.Courses.Commands.UploadLessonVideo;

[Permission(Permissions.Courses.Update)]
public sealed record UploadLessonVideoCommand(
    Guid CourseId,
    Guid SectionId,
    Guid LessonId,
    Stream VideoStream,
    string VideoFileName) : ICommand, ICourseCommand;
