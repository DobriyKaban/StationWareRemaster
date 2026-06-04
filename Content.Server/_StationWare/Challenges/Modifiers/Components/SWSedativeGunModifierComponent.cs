using Robust.Shared.Prototypes;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class SWSedativeGunModifierComponent : Component
{
    [DataField("gunPrototype")]
    public EntProtoId GunPrototype = "SWSedativeGun";

    [DataField("spawnedGuns")]
    public List<EntityUid> SpawnedGuns = new();
}
