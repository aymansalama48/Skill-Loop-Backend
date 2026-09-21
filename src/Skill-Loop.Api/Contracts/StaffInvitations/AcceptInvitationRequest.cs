namespace Skill_Loop.Api.Contracts.StaffInvitations;

public sealed record AcceptInvitationRequest(
    string InvitationToken,
    string FullName,
    string Password,
    string? PhoneNumber = null);
