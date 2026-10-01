using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Skill_Loop.Application.Features.Courses.Commands.UploadCourseThumbnail;

public sealed class UploadCourseThumbnailCommandHandler : ICommandHandler<UploadCourseThumbnailCommand, string>
{
    private readonly IFileStorage _fileStorage;

    public UploadCourseThumbnailCommandHandler(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<Result<string>> Handle(UploadCourseThumbnailCommand request, CancellationToken cancellationToken)
    {
        var uploadResult = await _fileStorage.UploadAsync(
            request.ThumbnailStream,
            request.ThumbnailFileName,
            $"Courses/{request.CourseId}/Thumbnail");

        if (uploadResult.IsFailure)
        {
            return Result<string>.Failure(uploadResult.Errors.First());
        }

        return Result<string>.Success(uploadResult.Data!);
    }
}
