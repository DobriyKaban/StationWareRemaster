using Robust.Shared.Prototypes;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class SWHotPotatoModifierComponent : Component
{
    [DataField("potatoPrototype")]
    public EntProtoId PotatoPrototype = "HotPotato";

    [DataField("percentage")]
    public float Percentage = 0.25f;

    [DataField("providedPotatoes")]
    public List<EntityUid> ProvidedPotatoes = new();
}
