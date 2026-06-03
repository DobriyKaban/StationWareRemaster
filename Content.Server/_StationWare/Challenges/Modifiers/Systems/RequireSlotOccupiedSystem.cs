using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class RequireSlotOccupiedSystem : EntitySystem
{
    [Dependency] private StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private InventorySystem _invSystem = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<RequireSlotOccupiedComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, RequireSlotOccupiedComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var player in args.Players)
        {
            _invSystem.TryGetSlotEntity(player, component.Slot, out var slotEntity);

            // assume we need to check prototype
            if (slotEntity == null)
                continue;

            if (component.Whitelist == null || _whitelist.IsValid(component.Whitelist, slotEntity.Value))
                _stationWareChallenge.SetPlayerChallengeState(player, uid, true, args.Component);
        }
    }
}
