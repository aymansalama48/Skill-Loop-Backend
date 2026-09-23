namespace Skill_Loop.Api.Contracts.Sessions;

public sealed record ReorderSessionMaterialsRequest(IReadOnlyList<SessionMaterialOrderItem> Materials);

public sealed record SessionMaterialOrderItem(Guid MaterialId, int SortOrder);
