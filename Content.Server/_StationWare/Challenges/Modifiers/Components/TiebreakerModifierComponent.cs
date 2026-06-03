namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for the tiebreaker challenge
/// </summary>
[RegisterComponent]
public sealed partial class TiebreakerModifierComponent : Component
{

}

[RegisterComponent]
public sealed partial class TiebreakerTrackerComponent : Component
{
    /// <summary>
    /// The challenge entity for the tiebreaker
    /// </summary>
    [DataField("challenge")] public EntityUid Challenge;
}
