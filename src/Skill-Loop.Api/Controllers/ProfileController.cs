namespace Skill_Loop.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Profile;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ChangePassword;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfile;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetMyProfile;

[Route("api/v1/[controller]")]
[Authorize]       // حماية الكنترولر بالكامل
public class ProfileController : BaseApiController
{
    [HttpGet]
    public async Task<IResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyAccountProfileQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IResult> UpdateMyProfile(
        [FromBody] UpdateMyAccountProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMyAccountProfileCommand(
            request.FirstName,
            request.LastName,
            request.PhoneNumber);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// رفع وتحديث الصورة الشخصية للمستخدم الحالي
    /// </summary>
    [HttpPatch("picture")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UpdateMyProfilePicture(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { message = "يرجى اختيار ملف صالح للرفع." });
        }

        await using var stream = file.OpenReadStream();

        var command = new UpdateMyProfilePictureCommand(stream, file.FileName);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost("change-password")]
    public async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}