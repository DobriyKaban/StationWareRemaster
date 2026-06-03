using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Buckle.Systems;
using Content.Shared.Buckle.Components;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class BuckledWinModifierSystem : EntitySystem
{
    [Dependency] private BuckleSystem _buckle = default!;
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BuckledWinModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, BuckledWinModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var player in args.Players)
        {
            if (TryComp<BuckleComponent>(player, out var buckle) && buckle.Buckled)
            {
                _buckle.TryUnbuckle((player, buckle), player, true);
                _stationWareChallenge.SetPlayerChallengeState(player, uid, true, args.Component);
            }
        }

        var query = EntityQueryEnumerator<BuckleComponent>();
        while (query.MoveNext(out var buckle, out _))
        {
            if (!Terminating(buckle) && Exists(buckle))
                Del(buckle);
        }
    }
}
