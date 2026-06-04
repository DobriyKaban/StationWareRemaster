using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;

using Content.Shared.Gibbing;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class SWPortalRouletteModifierSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private SharedTransformSystem _transformSystem = default!;
    [Dependency] private Content.Server.Explosion.EntitySystems.ExplosionSystem _explosionSystem = default!;
    [Dependency] private GibbingSystem _gib = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SWPortalRouletteModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SWPortalRouletteModifierComponent, ChallengeEndEvent>(OnChallengeEnd);

        SubscribeLocalEvent<SWPortalComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<SWPortalComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnChallengeStart(EntityUid uid, SWPortalRouletteModifierComponent component, ref ChallengeStartEvent args)
    {
        // Spawn 2 portals per player to make sure there are plenty of options
        var portalCount = args.Players.Count * 2;

        // Just get the map grid from one of the players
        if (args.Players.Count == 0)
            return;

        var firstPlayerCoords = Transform(args.Players[0]).Coordinates;

        for (var i = 0; i < portalCount; i++)
        {
            var offset = _random.NextVector2(3f);
            var spawnCoords = firstPlayerCoords.Offset(offset);

            var portal = Spawn(component.PortalPrototype, spawnCoords);
            var portalComp = EnsureComp<SWPortalComponent>(portal);
            portalComp.ChallengeUid = uid;
            portalComp.IsSafe = _random.Prob(0.5f); // 50% safe, 50% boom

            component.SpawnedPortals.Add(portal);
        }
    }

    private void OnCollide(EntityUid uid, SWPortalComponent component, ref StartCollideEvent args)
    {
        TryTriggerPortal(uid, component, args.OtherEntity);
    }

    private void OnActivate(EntityUid uid, SWPortalComponent component, ActivateInWorldEvent args)
    {
        TryTriggerPortal(uid, component, args.User);
    }

    private void TryTriggerPortal(EntityUid portalUid, SWPortalComponent component, EntityUid player)
    {
        if (!Exists(component.ChallengeUid))
            return;

        if (!TryComp<StationWareChallengeComponent>(component.ChallengeUid, out var challenge))
            return;

        if (!TryComp<ActorComponent>(player, out var actor))
            return;

        var userId = actor.PlayerSession.UserId;

        if (!challenge.Completions.ContainsKey(userId))
            return;

        // If player has already completed/failed, ignore
        if (challenge.Completions[userId] != null)
            return;

        if (component.IsSafe)
        {
            // Safe!
            _popup.PopupEntity(Loc.GetString("sw-portal-safe"), player, player, PopupType.Medium);
            _audio.PlayPvs("/Audio/Effects/teleport_arrival.ogg", player);

            var xform = Transform(player);
            var targetCoords = xform.Coordinates.Offset(_random.NextVector2(3f));
            _transformSystem.SetCoordinates(player, targetCoords);

            _stationWareChallenge.SetPlayerChallengeState(player, component.ChallengeUid, true, challenge);
        }
        else
        {
            // Boom!
            _popup.PopupEntity(Loc.GetString("sw-portal-failed"), player, player, PopupType.LargeCaution);
            _gib.Gib(player, true);

            _stationWareChallenge.SetPlayerChallengeState(player, component.ChallengeUid, false, challenge);
        }

        Del(portalUid);
    }

    private void TriggerExplosion(EntityUid portalUid)
    {
        var xform = Transform(portalUid);
        _explosionSystem.QueueExplosion(xform.MapPosition, "Default", 10f, 1f, 5f, cause: portalUid);
    }

    private void OnChallengeEnd(EntityUid uid, SWPortalRouletteModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var portal in component.SpawnedPortals)
        {
            if (Exists(portal))
                Del(portal);
        }
        component.SpawnedPortals.Clear();
    }
}
