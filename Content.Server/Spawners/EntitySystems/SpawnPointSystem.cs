using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Map.Components;

namespace Content.Server.Spawners.EntitySystems;

public sealed partial class SpawnPointSystem : EntitySystem
{
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StationSystem _stationSystem = default!;
    [Dependency] private StationSpawningSystem _stationSpawning = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult != null)
            return;

        // TODO: Cache all this if it ends up important.
        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();

        while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (args.Station != null && args.Station.Value.IsValid() && _stationSystem.GetOwningStation(uid, xform) != args.Station)
                continue;

            if (_gameTicker.Preset?.ID == "StationWare" || _gameTicker.CurrentPreset?.ID == "StationWare")
            {
                if (spawnPoint.SpawnType == SpawnPointType.Job || spawnPoint.SpawnType == SpawnPointType.LateJoin)
                {
                    possiblePositions.Add(xform.Coordinates);
                    continue;
                }
            }

            if (_gameTicker.RunLevel == GameRunLevel.InRound && spawnPoint.SpawnType == SpawnPointType.LateJoin)
            {
                possiblePositions.Add(xform.Coordinates);
            }

            if (_gameTicker.RunLevel != GameRunLevel.InRound &&
                spawnPoint.SpawnType == SpawnPointType.Job &&
                (args.Job == null || spawnPoint.Job == null || spawnPoint.Job == args.Job))
            {
                possiblePositions.Add(xform.Coordinates);
            }
        }

        // StationWare fallbacks if no standard spawn points were found
        if (possiblePositions.Count == 0 && (_gameTicker.Preset?.ID == "StationWare" || _gameTicker.CurrentPreset?.ID == "StationWare"))
        {
            // 1. Try MapPlayerSpawnerComponent (markers) on the station
            var markers = EntityQueryEnumerator<MapPlayerSpawnerComponent, TransformComponent>();
            while (markers.MoveNext(out var uid, out _, out var xform))
            {
                if (args.Station != null && _stationSystem.GetOwningStation(uid, xform) != args.Station)
                    continue;

                possiblePositions.Add(xform.Coordinates);
            }

            // 2. Try grids associated with the station
            if (possiblePositions.Count == 0 && args.Station != null)
            {
                if (TryComp<StationDataComponent>(args.Station, out var stationData))
                {
                    foreach (var grid in stationData.Grids)
                    {
                        var gridXform = Transform(grid);
                        possiblePositions.Add(gridXform.Coordinates);
                    }
                }
            }

            // 3. Try any map grid
            if (possiblePositions.Count == 0)
            {
                var gridQuery = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
                while (gridQuery.MoveNext(out _, out _, out var xform))
                {
                    possiblePositions.Add(xform.Coordinates);
                }
            }
        }

        if (possiblePositions.Count == 0)
        {
            // Ok we've still not returned, but we need to put them /somewhere/.
            // TODO: Refactor gameticker spawning code so we don't have to do this!
            var points2 = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();

            if (points2.MoveNext(out _, out var xform))
            {
                Log.Error($"Unable to pick a valid spawn point, picking random spawner as a backup.\nRunLevel: {_gameTicker.RunLevel} Station: {ToPrettyString(args.Station)} Job: {args.Job}");
                possiblePositions.Add(xform.Coordinates);
            }
            else
            {
                Log.Error($"No spawn points were available!\nRunLevel: {_gameTicker.RunLevel} Station: {ToPrettyString(args.Station)} Job: {args.Job}");
                return;
            }
        }

        var spawnLoc = _random.Pick(possiblePositions);

        args.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            args.Job,
            args.HumanoidCharacterProfile,
            args.Station);
    }
}
