using System.Linq;
using System.Numerics;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class LoadMapModifierSystem : EntitySystem
{
    [Dependency] private IMapManager _map = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MapLoaderSystem _mapLoader = default!;
    [Dependency] private ResetPositionModifierSystem _resetPositionModifier = default!;
    [Dependency] private TransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<LoadMapModifierComponent, ChallengeInitEvent>(OnChallengeInit);
        SubscribeLocalEvent<LoadMapModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<LoadMapModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeInit(EntityUid uid, LoadMapModifierComponent component, ref ChallengeInitEvent args)
    {
        component.Map = _map.CreateMap();
        if (!_mapLoader.TryLoadMapWithId(component.Map.Value, component.MapPath, out var map, out _))
            return;

        var mapEnt = _map.GetMapEntityId(component.Map.Value);
        Dirty(mapEnt, Comp<MapComponent>(mapEnt));
    }

    private void OnChallengeStart(EntityUid uid, LoadMapModifierComponent component, ref ChallengeStartEvent args)
    {
        if (component.Map == null)
            return;

        var mapEnt = _map.GetMapEntityId(component.Map.Value);

        var validSpawns = new List<(EntityUid uid, Vector2)>();
        var query = EntityQueryEnumerator<MapPlayerSpawnerComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out _, out var xform))
        {
            if (xform.MapID == component.Map)
                validSpawns.Add((ent, _transform.GetWorldPosition(xform)));
        }

        if (!validSpawns.Any())
            return;

        foreach (var player in args.Players)
        {
            var (spawn, pos) = _random.Pick(validSpawns);
            var playerXform = Transform(player);
            _transform.SetParent(player, playerXform, mapEnt);
            _transform.SetWorldPosition(playerXform, pos);
            _transform.SetCoordinates(player, playerXform, new EntityCoordinates(spawn, 0, 0));
        }
    }

    private void OnChallengeEnd(EntityUid uid, LoadMapModifierComponent component, ref ChallengeEndEvent args)
    {
        _resetPositionModifier.ResetPositions(args.Players);

        if (component.Map != null)
            _map.DeleteMap(component.Map.Value);
    }
}
