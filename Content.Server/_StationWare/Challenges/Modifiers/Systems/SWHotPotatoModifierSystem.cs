using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Hands.Systems;
using Content.Shared.Bed.Sleep;
using Content.Shared.Hands.Components;
using Content.Shared.HotPotato;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class SWHotPotatoModifierSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private SharedHotPotatoSystem _hotPotato = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SWHotPotatoModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SWHotPotatoModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
        SubscribeLocalEvent<SWHotPotatoModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeStart(EntityUid uid, SWHotPotatoModifierComponent component, ref ChallengeStartEvent args)
    {
        if (args.Players.Count == 0)
            return;

        var count = (int) Math.Max(1, Math.Round(args.Players.Count * component.Percentage));
        var shuffledPlayers = args.Players.ToList();
        _random.Shuffle(shuffledPlayers);
        var chosenPlayers = shuffledPlayers.Take(count);

        var handsQuery = GetEntityQuery<HandsComponent>();

        foreach (var player in chosenPlayers)
        {
            if (!handsQuery.TryGetComponent(player, out var handsComponent))
                continue;

            var xform = Transform(player);
            var potato = Spawn(component.PotatoPrototype, xform.Coordinates);

            if (TryComp<HotPotatoComponent>(potato, out var potatoComp))
            {
                EnsureComp<ActiveHotPotatoComponent>(potato);
                _hotPotato.SetCanTransfer(potato, false, potatoComp);
            }

            if (_hands.TryPickupAnyHand(player, potato, false, handsComp: handsComponent))
            {
                component.ProvidedPotatoes.Add(potato);
            }
            else
            {
                Del(potato);
            }
        }
    }

    private void OnBeforeChallengeEnd(EntityUid uid, SWHotPotatoModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        var handsQuery = GetEntityQuery<HandsComponent>();
        var hotPotatoQuery = GetEntityQuery<HotPotatoComponent>();

        foreach (var player in args.Players)
        {
            if (!handsQuery.TryGetComponent(player, out var hands))
                continue;

            var holdsPotato = false;
            foreach (var item in _hands.EnumerateHeld((player, hands)))
            {
                if (hotPotatoQuery.HasComponent(item))
                {
                    holdsPotato = true;
                    break;
                }
            }

            if (holdsPotato)
            {
                _stationWareChallenge.SetPlayerChallengeState(player, uid, false, args.Component);
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, SWHotPotatoModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var potato in component.ProvidedPotatoes)
        {
            if (Exists(potato))
                Del(potato);
        }
        component.ProvidedPotatoes.Clear();
    }
}
