using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._StationWare.Points;
using Robust.Shared.GameStates;

namespace Content.Client._StationWare.Points;

public sealed class PointSystem : SharedPointSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationWarePointManagerComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, StationWarePointManagerComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not StationWarePointManagerComponentState state)
            return;
        component.Points = new(state.Points);
    }

    public override bool TryGetPointManager([NotNullWhen(true)] ref StationWarePointManagerComponent? component)
    {
        if (component != null)
            return true;

        var query = EntityQuery<StationWarePointManagerComponent>().ToList();
        component = query.FirstOrDefault();
        return component != null;
    }
}
