using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._StationWare.Challenges;
using Content.Shared._StationWare.Points;
using Robust.Server.GameStates;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Server._StationWare.Points;

public sealed partial class PointSystem : SharedPointSystem
{
    [Dependency] private PvsOverrideSystem _pvs = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationWarePointManagerComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<StationWarePointManagerComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<PlayerChallengeStateSetEvent>(OnPlayerChallengeStateSet);
    }

    private void OnGetState(EntityUid uid, StationWarePointManagerComponent component, ref ComponentGetState args)
    {
        args.State = new StationWarePointManagerComponentState(component.Points);
    }

    private void OnInit(EntityUid uid, StationWarePointManagerComponent component, ComponentStartup args)
    {
        _pvs.AddGlobalOverride(uid);
    }

    private void OnPlayerChallengeStateSet(ref PlayerChallengeStateSetEvent ev)
    {
        StationWarePointManagerComponent? manager = null;
        if (!TryGetPointManager(ref manager))
            return;

        EnsurePointInfo(manager, ev.Player);
        if (ev.Won)
            AdjustPoints(ev.Player, ev.Points, manager);
    }

    public override bool TryGetPointManager([NotNullWhen(true)] ref StationWarePointManagerComponent? component)
    {
        if (component != null)
            return true;

        var query = EntityQuery<StationWarePointManagerComponent>().ToList();
        component = !query.Any() ? CreatePointManager() : query.First();
        return true;
    }

    public StationWarePointManagerComponent CreatePointManager()
    {
        var manager = Spawn(null, MapCoordinates.Nullspace);
        return EnsureComp<StationWarePointManagerComponent>(manager);
    }
}
