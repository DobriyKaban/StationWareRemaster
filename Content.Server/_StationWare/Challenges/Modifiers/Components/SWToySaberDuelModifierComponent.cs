using Robust.Shared.Prototypes;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class SWToySaberDuelModifierComponent : Component
{
    [DataField("saberPrototype")]
    public EntProtoId SaberPrototype = "ToySword";

    [DataField("spawnedSabers")]
    public List<EntityUid> SpawnedSabers = new();
}
