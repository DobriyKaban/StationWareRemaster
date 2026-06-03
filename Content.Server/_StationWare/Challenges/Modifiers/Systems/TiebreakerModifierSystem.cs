using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Gibbing;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

/// <summary>
/// This handles the tiebreaker challenge
/// </summary>
public sealed partial class TiebreakerModifierSystem : EntitySystem
{
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private GibbingSystem _gib = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TiebreakerModifierComponent, ChallengeStartEvent>(OnChallengeStart);
    }

    private void OnChallengeStart(EntityUid uid, TiebreakerModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            if (TryComp<TiebreakerTrackerComponent>(player, out var trackerComponent))
            {
                trackerComponent.Challenge = uid;
            }
            else
            {
                _stationWareChallenge.SetPlayerChallengeState(player, uid, false);
                _gib.Gib(player, true);
            }
        }
    }
}
