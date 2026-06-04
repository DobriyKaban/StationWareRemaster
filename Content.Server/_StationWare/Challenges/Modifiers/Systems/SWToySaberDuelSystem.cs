using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.Hands.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class SWToySaberDuelSystem : EntitySystem
{
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SWToySaberDuelModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SWToySaberDuelModifierComponent, ChallengeEndEvent>(OnChallengeEnd);

        SubscribeLocalEvent<SWToySaberComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnChallengeStart(EntityUid uid, SWToySaberDuelModifierComponent component, ref ChallengeStartEvent args)
    {
        var handsQuery = GetEntityQuery<HandsComponent>();

        foreach (var player in args.Players)
        {
            var xform = Transform(player);
            var saber = Spawn(component.SaberPrototype, xform.Coordinates);

            var saberComp = EnsureComp<SWToySaberComponent>(saber);
            saberComp.ChallengeUid = uid;

            if (handsQuery.TryGetComponent(player, out var handsComponent))
            {
                if (_hands.TryPickupAnyHand(player, saber, false, handsComp: handsComponent))
                {
                    component.SpawnedSabers.Add(saber);
                }
                else
                {
                    Del(saber);
                }
            }
            else
            {
                Del(saber);
            }
        }
    }

    private void OnMeleeHit(EntityUid uid, SWToySaberComponent component, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        if (!Exists(component.ChallengeUid))
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            if (hitEntity == args.User)
                continue;

            if (TryComp<StationWareChallengeComponent>(component.ChallengeUid, out var challenge))
            {
                if (!TryComp<ActorComponent>(hitEntity, out var actor))
                    continue;

                var userId = actor.PlayerSession.UserId;

                if (!challenge.Completions.ContainsKey(userId))
                    continue;

                // If already failed, ignore
                if (challenge.Completions[userId] == false)
                    continue;

                // Mark as failed
                _stationWareChallenge.SetPlayerChallengeState(hitEntity, component.ChallengeUid, false, challenge);

                // Knock down
                _stunSystem.TryUpdateParalyzeDuration(hitEntity, TimeSpan.FromSeconds(5));

                // Disarm
                if (TryComp<HandsComponent>(hitEntity, out var hands))
                {
                    _hands.DropAll((hitEntity, hands));
                }

                // Popup & Sound
                _popup.PopupEntity(Loc.GetString("sw-toy-saber-knocked-out"), hitEntity, PopupType.LargeCaution);
                _audio.PlayPvs("/Audio/Weapons/laser_saber_hit.ogg", hitEntity);
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, SWToySaberDuelModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var saber in component.SpawnedSabers)
        {
            if (Exists(saber))
                Del(saber);
        }
        component.SpawnedSabers.Clear();
    }
}
