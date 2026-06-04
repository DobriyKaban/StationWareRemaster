using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared._StationWare;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.Bed.Sleep;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Robust.Shared.Audio.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class SWSedativeGunSystem : EntitySystem
{
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private SleepingSystem _sleepingSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SWSedativeGunModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SWSedativeGunModifierComponent, ChallengeEndEvent>(OnChallengeEnd);

        SubscribeLocalEvent<SWSedativeDartComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnChallengeStart(EntityUid uid, SWSedativeGunModifierComponent component, ref ChallengeStartEvent args)
    {
        var handsQuery = GetEntityQuery<HandsComponent>();

        foreach (var player in args.Players)
        {
            var xform = Transform(player);
            var gun = Spawn(component.GunPrototype, xform.Coordinates);

            if (handsQuery.TryGetComponent(player, out var handsComponent))
            {
                if (_hands.TryPickupAnyHand(player, gun, false, handsComp: handsComponent))
                {
                    component.SpawnedGuns.Add(gun);
                }
                else
                {
                    Del(gun);
                }
            }
            else
            {
                Del(gun);
            }
        }
    }

    private void OnProjectileHit(EntityUid uid, SWSedativeDartComponent component, ref ProjectileHitEvent args)
    {
        var target = args.Target;

        var query = EntityQueryEnumerator<StationWareChallengeComponent>();
        while (query.MoveNext(out var challengeUid, out var challenge))
        {
            if (!TryComp<ActorComponent>(target, out var actor))
                continue;

            var userId = actor.PlayerSession.UserId;

            if (!challenge.Completions.ContainsKey(userId))
                continue;

            // If already failed, ignore
            if (challenge.Completions[userId] == false)
                continue;

            // Mark target as failed
            _stationWareChallenge.SetPlayerChallengeState(target, challengeUid, false, challenge);

            // Put target to sleep
            _sleepingSystem.TrySleeping(target);

            // Drop target's held items
            if (TryComp<HandsComponent>(target, out var hands))
            {
                _hands.DropAll((target, hands));
            }

            // Popup
            _popup.PopupEntity(Loc.GetString("sw-sedative-asleep"), target, PopupType.LargeCaution);
        }
    }

    private void OnChallengeEnd(EntityUid uid, SWSedativeGunModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var gun in component.SpawnedGuns)
        {
            if (Exists(gun))
                Del(gun);
        }
        component.SpawnedGuns.Clear();

        // Also clean up any sleeping players to wake them up
        foreach (var player in args.Players)
        {
            if (HasComp<SleepingComponent>(player))
            {
                _sleepingSystem.TryWaking(player, force: true);
            }
        }
    }
}
