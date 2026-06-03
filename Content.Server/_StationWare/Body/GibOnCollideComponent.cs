namespace Content.Server._StationWare.Body;

[RegisterComponent]
public sealed partial class GibOnCollideComponent : Component
{
    [DataField("allowMultipleHits")]
    public bool AllowMultipleHits = true;
}
