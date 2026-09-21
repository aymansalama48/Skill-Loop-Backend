namespace Skill_Loop.Application.Common.Abstractions.Identity.Invitations;

public sealed record InvitationDetailsDto(
    Guid InvitationId,
    string Email,
    string Role,
    string AdminName,

    bool IsValid);
