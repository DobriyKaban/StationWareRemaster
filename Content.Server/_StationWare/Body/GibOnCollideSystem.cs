using Content.Shared.Body;
using Content.Shared.Gibbing;
using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Events;

namespace Content.Server._StationWare.Body;

public sealed partial class GibOnCollideSystem : EntitySystem
{
    [Dependency] private GibbingSystem _gib = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GibOnCollideComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, GibOnCollideComponent component, ref StartCollideEvent args)
    {
        var otherEnt = args.OtherEntity;
        if (!HasComp<MobStateComponent>(otherEnt) || !TryComp<BodyComponent>(otherEnt, out var body))
            return;
        _gib.Gib(otherEnt, true);
        if (!component.AllowMultipleHits)
            Del(uid);
    }
}
