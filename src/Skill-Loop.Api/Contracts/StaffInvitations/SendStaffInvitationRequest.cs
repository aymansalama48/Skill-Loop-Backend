namespace Skill_Loop.Api.Contracts.StaffInvitations;

public sealed record SendStaffInvitationRequest(
    string Email,
    string Role);
