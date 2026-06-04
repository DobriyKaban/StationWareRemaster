namespace Content.Shared._StationWare;

[RegisterComponent]
public sealed partial class SWDefusalBombComponent : Component
{
    [DataField("correctColor")]
    public string CorrectColor = "red";

    [DataField("defused")]
    public bool Defused = false;

    [DataField("challengeUid")]
    public EntityUid ChallengeUid = EntityUid.Invalid;

    [DataField("ownerPlayer")]
    public EntityUid OwnerPlayer = EntityUid.Invalid;
}
