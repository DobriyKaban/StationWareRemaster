namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class SWPortalComponent : Component
{
    [DataField("challengeUid")]
    public EntityUid ChallengeUid = EntityUid.Invalid;

    [DataField("isSafe")]
    public bool IsSafe = true;
}
