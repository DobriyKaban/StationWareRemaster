namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed partial class SWToySaberComponent : Component
{
    [DataField("challengeUid")]
    public EntityUid ChallengeUid = EntityUid.Invalid;
}
