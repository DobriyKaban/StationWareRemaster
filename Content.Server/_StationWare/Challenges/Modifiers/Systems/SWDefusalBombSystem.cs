using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared._StationWare;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class SWDefusalBombSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private SharedToolSystem _toolSystem = default!;
    [Dependency] private Content.Server.Explosion.EntitySystems.ExplosionSystem _explosionSystem = default!;

    private static readonly string[] Colors = { "red", "blue", "green" };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SWDefusalBombModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SWDefusalBombModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
        SubscribeLocalEvent<SWDefusalBombModifierComponent, ChallengeEndEvent>(OnChallengeEnd);

        SubscribeLocalEvent<SWDefusalBombComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }

    private void OnChallengeStart(EntityUid uid, SWDefusalBombModifierComponent component, ref ChallengeStartEvent args)
    {
        var handsQuery = GetEntityQuery<HandsComponent>();

        foreach (var player in args.Players)
        {
            var xform = Transform(player);

            // Spawn and give wirecutter
            var wirecutter = Spawn(component.WirecutterPrototype, xform.Coordinates);
            if (handsQuery.TryGetComponent(player, out var handsComponent))
            {
                if (_hands.TryPickupAnyHand(player, wirecutter, false, handsComp: handsComponent))
                {
                    component.ProvidedWirecutters.Add(wirecutter);
                }
                else
                {
                    Del(wirecutter);
                }
            }
            else
            {
                Del(wirecutter);
            }

            // Spawn bomb
            var bombCoords = xform.Coordinates.Offset(_random.NextVector2(0.6f));
            var bomb = Spawn(component.BombPrototype, bombCoords);
            var bombComp = EnsureComp<SWDefusalBombComponent>(bomb);
            bombComp.ChallengeUid = uid;
            bombComp.OwnerPlayer = player;
            bombComp.CorrectColor = _random.Pick(Colors);

            component.SpawnedBombs.Add(bomb);

            // Show localized instruction to the player
            var colorName = Loc.GetString($"sw-defusal-color-{bombComp.CorrectColor}");
            var message = Loc.GetString("sw-defusal-popup-instruction", ("color", colorName));
            _popup.PopupEntity(message, player, player, PopupType.Large);
        }
    }

    private void OnGetVerbs(EntityUid uid, SWDefusalBombComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || component.Defused)
            return;

        // Check if user is holding a wirecutter
        var hasWirecutter = false;
        if (TryComp<HandsComponent>(args.User, out var hands))
        {
            foreach (var item in _hands.EnumerateHeld((args.User, hands)))
            {
                if (_toolSystem.HasQuality(item, SharedToolSystem.CutQuality) ||
                    MetaData(item).EntityPrototype?.ID == "Wirecutter")
                {
                    hasWirecutter = true;
                    break;
                }
            }
        }

        foreach (var color in Colors)
        {
            var targetColor = color;
            var colorName = Loc.GetString($"sw-defusal-color-{targetColor}");
            var verbText = Loc.GetString("sw-defusal-verb-cut", ("color", colorName));

            args.Verbs.Add(new InteractionVerb
            {
                Text = verbText,
                Act = () =>
                {
                    if (!hasWirecutter)
                    {
                        _popup.PopupEntity(Loc.GetString("sw-defusal-no-wirecutter"), uid, args.User, PopupType.SmallCaution);
                        return;
                    }

                    TryCutWire(uid, component, targetColor, args.User);
                },
                Priority = 5
            });
        }
    }

    private void TryCutWire(EntityUid bombUid, SWDefusalBombComponent component, string color, EntityUid user)
    {
        if (component.Defused)
            return;

        if (color == component.CorrectColor)
        {
            // Success!
            component.Defused = true;
            _popup.PopupEntity(Loc.GetString("sw-defusal-bomb-defused"), bombUid, user, PopupType.Medium);
            _audio.PlayPvs("/Audio/Machines/Nuke/general_beep.ogg", bombUid);

            if (Exists(component.ChallengeUid) && TryComp<StationWareChallengeComponent>(component.ChallengeUid, out var challenge))
            {
                _stationWareChallenge.SetPlayerChallengeState(user, component.ChallengeUid, true, challenge);
            }
        }
        else
        {
            // Boom!
            _popup.PopupEntity(Loc.GetString("sw-defusal-bomb-failed"), bombUid, user, PopupType.LargeCaution);
            TriggerBombExplosion(bombUid);

            if (Exists(component.ChallengeUid) && TryComp<StationWareChallengeComponent>(component.ChallengeUid, out var challenge))
            {
                _stationWareChallenge.SetPlayerChallengeState(user, component.ChallengeUid, false, challenge);
            }

            Del(bombUid);
        }
    }

    private void TriggerBombExplosion(EntityUid bombUid)
    {
        var xform = Transform(bombUid);
        _explosionSystem.QueueExplosion(xform.MapPosition, "Default", 10f, 1f, 5f, cause: bombUid);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, SWDefusalBombModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var bomb in component.SpawnedBombs)
        {
            if (!Exists(bomb))
                continue;

            if (TryComp<SWDefusalBombComponent>(bomb, out var bombComp) && !bombComp.Defused)
            {
                // Explode for failing to defuse in time
                TriggerBombExplosion(bomb);

                if (Exists(bombComp.OwnerPlayer))
                {
                    _stationWareChallenge.SetPlayerChallengeState(bombComp.OwnerPlayer, uid, false, args.Component);
                }
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, SWDefusalBombModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var bomb in component.SpawnedBombs)
        {
            if (Exists(bomb))
                Del(bomb);
        }
        component.SpawnedBombs.Clear();

        foreach (var wirecutter in component.ProvidedWirecutters)
        {
            if (Exists(wirecutter))
                Del(wirecutter);
        }
        component.ProvidedWirecutters.Clear();
    }
}
