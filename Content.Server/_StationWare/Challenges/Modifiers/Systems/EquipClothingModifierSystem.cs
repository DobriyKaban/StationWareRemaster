using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

/// <summary>
/// This handles <see cref="EquipClothingModifierComponent"/>
/// </summary>
public sealed partial class EquipClothingModifierSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private InventorySystem _inventory = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<EquipClothingModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<EquipClothingModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeStart(EntityUid uid, EquipClothingModifierComponent component, ref ChallengeStartEvent args)
    {
        if (!args.Players.Any())
            return;

        var playerAmount = (int) Math.Clamp(MathF.Round(args.Players.Count * component.ReceivingPercentage), 1, args.Players.Count);

        foreach (var player in args.Players.Take(playerAmount))
        {
            var xform = Transform(player);
            if (!TryComp<InventoryComponent>(player, out var inventory))
                continue;
            if (!_inventory.TryGetSlots(player, out var slots))
                continue;
            var spawns = EntitySpawnCollection.GetSpawns(component.Spawns, _random);
            foreach (var spawn in spawns)
            {
                var clothing = Spawn(spawn, xform.Coordinates);
                if (slots.Any(slot => _inventory.CanEquip(player, clothing, slot.Name, out _, slot) &&
                                      _inventory.TryEquip(player, clothing, slot.Name, true, inventory: inventory)))
                {
                    component.SpawnedEntities.Add(clothing);
                }
                else
                {
                    Del(clothing);
                }
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, EquipClothingModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var ent in component.SpawnedEntities)
        {
            Del(ent);
        }
    }
}
