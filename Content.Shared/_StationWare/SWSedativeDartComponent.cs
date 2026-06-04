namespace Content.Shared._StationWare;

[RegisterComponent]
public sealed partial class SWSedativeDartComponent : Component
{
    [DataField("challengeUid")]
    public EntityUid ChallengeUid = EntityUid.Invalid;
}
