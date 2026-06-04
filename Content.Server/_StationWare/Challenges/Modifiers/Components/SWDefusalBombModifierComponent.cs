using Robust.Shared.Prototypes;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class SWDefusalBombModifierComponent : Component
{
    [DataField("bombPrototype")]
    public EntProtoId BombPrototype = "SWDefusalBomb";

    [DataField("wirecutterPrototype")]
    public EntProtoId WirecutterPrototype = "Wirecutter";

    [DataField("spawnedBombs")]
    public List<EntityUid> SpawnedBombs = new();

    [DataField("providedWirecutters")]
    public List<EntityUid> ProvidedWirecutters = new();
}
