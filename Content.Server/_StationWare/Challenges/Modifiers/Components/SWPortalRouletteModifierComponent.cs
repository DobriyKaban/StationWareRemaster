using Robust.Shared.Prototypes;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class SWPortalRouletteModifierComponent : Component
{
    [DataField("portalPrototype")]
    public EntProtoId PortalPrototype = "PortalBlue";

    [DataField("spawnedPortals")]
    public List<EntityUid> SpawnedPortals = new();
}
