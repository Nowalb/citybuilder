using System.Collections.Generic;
using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Unity adapter: drives simulation tick, auto-growth and visual sync.
    /// </summary>
    public sealed class SimulationBootstrap : MonoBehaviour
    {
        [Header("Hex Grid")]
        [SerializeField] private int hexMapSideLength = 30;
        [SerializeField] private float hexSize = 1f;
        [SerializeField] private int terrainSeed = 1337;
        [SerializeField, Range(0f, 0.45f)] private float waterThreshold = 0.12f;

        [Header("Simulation")]
        [SerializeField] private float tickIntervalSeconds = 1f;
        [SerializeField] private int maxCitizenDots = 300;

        [Header("Auto Growth")]
        [SerializeField] private int roadsPerTick = 3;
        [SerializeField] private int buildingsPerTick = 2;
        [SerializeField] private int buildingRoadSearchRadius = 2;

        [Header("References")]
        [SerializeField] private HexGridRenderer hexGridRenderer;

        private GridSystem _grid;
        private CitySimulation _simulation;
        private Camera _mainCamera;

        private readonly Dictionary<Building, GameObject> _buildingViews = new();
        private readonly Dictionary<Citizen, GameObject> _citizenViews = new();
        private readonly List<HexCoord> _roadCoords = new();

        private float _elapsed;

        public GridSystem Grid => _grid;

        private void OnValidate()
        {
            hexMapSideLength = Mathf.Max(2, hexMapSideLength);
            hexSize = Mathf.Max(0.1f, hexSize);
            tickIntervalSeconds = Mathf.Max(0.1f, tickIntervalSeconds);
            maxCitizenDots = Mathf.Max(1, maxCitizenDots);
            roadsPerTick = Mathf.Max(1, roadsPerTick);
            buildingsPerTick = Mathf.Max(0, buildingsPerTick);
            buildingRoadSearchRadius = Mathf.Clamp(buildingRoadSearchRadius, 1, 8);
        }

        private void Awake()
        {
            if (hexGridRenderer == null)
            {
                hexGridRenderer = GetComponent<HexGridRenderer>();
            }

            if (hexGridRenderer == null)
            {
                hexGridRenderer = gameObject.AddComponent<HexGridRenderer>();
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            _grid = new GridSystem(hexMapSideLength, terrainSeed, waterThreshold, createHexShape: true);
            _simulation = new CitySimulation(_grid);

            hexGridRenderer.Initialize(_grid, hexSize);
            RefreshViews();
            RunTick();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            while (_elapsed >= tickIntervalSeconds)
            {
                _elapsed -= tickIntervalSeconds;
                RunAutoGrowthStep();
                RunTick();
                RefreshViews();
            }
        }

        public bool TryPickHex(Vector2 screenPosition, out HexCoord coord)
        {
            coord = default;
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return false;
                }
            }

            var ray = _mainCamera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(ray, out var enter))
            {
                return false;
            }

            var world = ray.GetPoint(enter);
            var snapped = HexGridMath.WorldToHex(world, hexSize);
            if (!_grid.Contains(snapped))
            {
                return false;
            }

            coord = snapped;
            return true;
        }

        public bool PlaceRoad(HexCoord coord)
        {
            var ok = _grid.PlaceRoad(coord);
            if (ok)
            {
                RegisterRoad(coord);
                hexGridRenderer.RefreshTile(_grid.GetTile(coord));
            }

            return ok;
        }

        public bool PlaceBuilding(HexCoord coord, BuildingType buildingType)
        {
            return _grid.PlaceBuilding(coord, buildingType);
        }

        public void RefreshViews()
        {
            RefreshBuildingViews();
            RefreshCitizenViews();
        }

        private void RunAutoGrowthStep()
        {
            EnsureSeedRoad();

            var placedRoads = 0;
            var safety = 0;
            while (placedRoads < roadsPerTick && safety < 400)
            {
                safety++;
                if (_roadCoords.Count == 0)
                {
                    break;
                }

                var source = _roadCoords[Random.Range(0, _roadCoords.Count)];
                foreach (var target in source.GetNeighbors())
                {
                    if (!_grid.Contains(target))
                    {
                        continue;
                    }

                    var tile = _grid.GetTile(target);
                    if (tile == null || !tile.IsBuildableTerrain || tile.IsRoad || tile.HasBuilding)
                    {
                        continue;
                    }

                    if (Random.value > 0.35f)
                    {
                        continue;
                    }

                    if (PlaceRoad(target))
                    {
                        placedRoads++;
                        break;
                    }
                }
            }

            var placedBuildings = 0;
            safety = 0;
            while (placedBuildings < buildingsPerTick && safety < 500)
            {
                safety++;
                if (_roadCoords.Count == 0)
                {
                    break;
                }

                var road = _roadCoords[Random.Range(0, _roadCoords.Count)];
                var candidates = _grid.GetTilesInRange(road, buildingRoadSearchRadius);

                for (var i = 0; i < candidates.Count && placedBuildings < buildingsPerTick; i++)
                {
                    var tile = candidates[i];
                    if (tile == null || tile.IsRoad || tile.HasBuilding || !tile.IsBuildableTerrain)
                    {
                        continue;
                    }

                    if (!_grid.HasAdjacentRoad(tile.Coord) || Random.value > 0.25f)
                    {
                        continue;
                    }

                    if (PlaceBuilding(tile.Coord, ResolveAutoBuildingType()))
                    {
                        placedBuildings++;
                    }
                }
            }
        }

        private void EnsureSeedRoad()
        {
            if (_roadCoords.Count > 0)
            {
                return;
            }

            var center = new HexCoord(0, 0);
            if (!PlaceRoad(center))
            {
                foreach (var tile in _grid.Tiles)
                {
                    if (tile.IsBuildableTerrain && PlaceRoad(tile.Coord))
                    {
                        break;
                    }
                }
            }
        }

        private void RegisterRoad(HexCoord coord)
        {
            for (var i = 0; i < _roadCoords.Count; i++)
            {
                if (_roadCoords[i].Equals(coord))
                {
                    return;
                }
            }

            _roadCoords.Add(coord);
        }

        private static BuildingType ResolveAutoBuildingType()
        {
            var value = Random.Range(0, 100);
            if (value < 42) return BuildingType.Residential;
            if (value < 62) return BuildingType.Industrial;
            if (value < 82) return BuildingType.Commercial;
            if (value < 89) return BuildingType.PoliceStation;
            if (value < 95) return BuildingType.FireStation;
            return BuildingType.Hospital;
        }

        private void RefreshBuildingViews()
        {
            foreach (var placement in _grid.Placements)
            {
                var building = placement.Building;
                if (_buildingViews.TryGetValue(building, out var view))
                {
                    var heightScale = 0.5f + building.Level * 0.4f;
                    var world = HexGridMath.HexToWorldPosition(placement.Coord, hexSize);
                    view.transform.localScale = new Vector3(hexSize * 1.4f, heightScale, hexSize * 1.4f);
                    view.transform.position = new Vector3(world.x, heightScale * 0.5f, world.z);
                    continue;
                }

                var buildingGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                buildingGo.name = $"Building_{building.BuildingType}_{placement.Coord.Q}_{placement.Coord.R}";
                buildingGo.transform.SetParent(transform, false);

                var initialHeight = 0.5f + building.Level * 0.4f;
                var worldPos = HexGridMath.HexToWorldPosition(placement.Coord, hexSize);
                buildingGo.transform.localScale = new Vector3(hexSize * 0.65f, initialHeight * 0.5f, hexSize * 0.65f);
                buildingGo.transform.position = new Vector3(worldPos.x, initialHeight * 0.5f, worldPos.z);
                buildingGo.GetComponent<Renderer>().material.color = ResolveBuildingColor(building.BuildingType);

                _buildingViews.Add(building, buildingGo);
            }
        }

        private void RefreshCitizenViews()
        {
            var visibleCount = 0;
            foreach (var citizen in _simulation.Citizens)
            {
                if (visibleCount >= maxCitizenDots)
                {
                    break;
                }

                if (!_citizenViews.TryGetValue(citizen, out var view))
                {
                    view = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    view.name = "CitizenDot";
                    view.transform.SetParent(transform, false);
                    view.transform.localScale = Vector3.one * (hexSize * 0.25f);
                    view.GetComponent<Renderer>().material.color = Color.white;
                    _citizenViews.Add(citizen, view);
                }

                var worldPos = HexGridMath.HexToWorldPosition(citizen.RoadCoord, hexSize);
                view.SetActive(true);
                view.transform.position = new Vector3(worldPos.x, 0.2f, worldPos.z);
                visibleCount++;
            }

            if (visibleCount < _citizenViews.Count)
            {
                var hiddenIndex = 0;
                foreach (var kv in _citizenViews)
                {
                    kv.Value.SetActive(hiddenIndex < visibleCount);
                    hiddenIndex++;
                }
            }
        }

        private static Color ResolveBuildingColor(BuildingType type)
        {
            return type switch
            {
                BuildingType.Residential => Color.green,
                BuildingType.Industrial => Color.yellow,
                BuildingType.Commercial => Color.blue,
                BuildingType.PoliceStation => new Color(0.1f, 0.3f, 0.9f),
                BuildingType.FireStation => Color.red,
                BuildingType.Hospital => Color.white,
                _ => Color.magenta
            };
        }

        private void RunTick()
        {
            _simulation.Tick();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"Tick {_simulation.TickCount} | Roads: {_roadCoords.Count} | Buildings: {_grid.Buildings.Count} | Residents: {_simulation.TotalResidents} | Jobs: {_simulation.TotalJobs} | " +
                $"Crime: {_simulation.CrimeIndex:0.0} | FireRisk: {_simulation.FireRiskIndex:0.0} | Health: {_simulation.HealthIndex:0.0} | " +
                $"Citizens: {_simulation.Citizens.Count} | Balance: {_simulation.Balance}");
#endif
        }
    }
}
