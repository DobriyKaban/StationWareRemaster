using Content.Server._StationWare.Challenges.Modifiers.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Components;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class ControlPointModifierSystem : EntitySystem
{
    [Dependency] private PhysicsSystem _physics = default!;
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ControlPointModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, ControlPointModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var (cp, body) in EntityQuery<ControlPointComponent, PhysicsComponent>())
        {
            foreach (var contact in _physics.GetContactingEntities(body.Owner, body))
            {
                var ent = contact;
                if (!args.Players.Contains(ent))
                    continue;
                _stationWareChallenge.SetPlayerChallengeState(ent, uid, true, args.Component);
            }
        }
    }
}
