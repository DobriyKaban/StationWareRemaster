namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class KeepMovingModifierComponent : Component
{

}

/// <summary>
/// A player that is playing <see cref="KeepMovingModifierComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class KeepMovingPlayerComponent : Component
{
    /// <summary>
    /// The challenge entity for the freeze modifier player
    /// </summary>
    [DataField("challenge")]
    public EntityUid Challenge;
}
