using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using System.Numerics;
using Robust.Shared.Maths;

namespace Content.Shared._StationWare.Weapons.Melee;

public sealed partial class KnockbackWeaponSystem : EntitySystem
{
    [Dependency] private ThrowingSystem _throwing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<KnockbackWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, KnockbackWeaponComponent component, MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var userXForm = Transform(args.User);
        foreach (var hit in args.HitEntities)
        {
            var hitXForm = Transform(hit);
            var direction = hitXForm.MapPosition.Position - userXForm.MapPosition.Position;
            if (direction == Vector2.Zero)
                continue;
            direction = direction.Normalized() * component.Distance;
            _throwing.TryThrow(hit, direction, component.Strength, args.User);
        }
    }
}
