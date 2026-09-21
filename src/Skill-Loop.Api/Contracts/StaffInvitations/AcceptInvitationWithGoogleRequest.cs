namespace Skill_Loop.Api.Contracts.StaffInvitations;

public sealed record AcceptInvitationWithGoogleRequest(
    string InvitationToken,
    string GoogleIdToken);
