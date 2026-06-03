using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._StationWare.Challenges;

/// <summary>
/// This is a prototype for a StationWare challenge.
/// </summary>
[Prototype]
public sealed partial class ChallengePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Whether or not the players win automatically or
    /// lose automatically.
    /// </summary>
    [DataField("winByDefault")]
    public bool WinByDefault;

    /// <summary>
    /// Tags for categorizing/filtering challenges
    /// </summary>
    [DataField("tags", customTypeSerializer: typeof(PrototypeIdListSerializer<TagPrototype>))]
    public List<string> Tags = new();

    /// <summary>
    /// How many points are awarded to the winners.
    /// </summary>
    [DataField("pointsAwarded")]
    public int PointsAwarded = 1;

    /// <summary>
    /// How long the challenge lasts.
    /// </summary>
    [DataField("duration")]
    public TimeSpan? Duration;

    /// <summary>
    /// A delay between the challenge announcement
    /// and the actual challenge beginning.
    /// </summary>
    [DataField("startDelay")]
    public TimeSpan StartDelay = TimeSpan.Zero;

    /// <summary>
    /// If true, the duration of a challenge will
    /// increase with speedup rather than decrease.
    /// </summary>
    [DataField("invertSpeedup")]
    public bool InvertSpeedup;

    /// <summary>
    /// The announcement played when the event starts
    /// </summary>
    [DataField("announcement")]
    public string Announcement = default!;

    /// <summary>
    /// The sound played when the event starts
    /// Defaults to the funny ding.
    /// </summary>
    [DataField("announcementSound")]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/_StationWare/event_ding.ogg");

    /// <summary>
    /// Components that are added to the challenge entity
    /// to dictate specific behaviors and conditions about
    /// the challenge itself.
    /// </summary>
    [DataField("challengeModifiers")]
    public ComponentRegistry ChallengeModifiers = new();
}
