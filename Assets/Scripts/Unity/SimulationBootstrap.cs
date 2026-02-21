using System.Collections.Generic;
using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Unity adapter: drives simulation and runtime visualization.
    /// </summary>
    public sealed class SimulationBootstrap : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;
        [SerializeField] private float tileSize = 1f;

        [Header("Simulation")]
        [SerializeField] private float tickIntervalSeconds = 1f;
        [SerializeField] private int buildingsPlacedPerTick = 120;

        [Header("Citizens")]
        [SerializeField] private int maxCitizenDots = 300;

        private GridSystem _grid;
        private CitySimulation _simulation;
        private readonly Dictionary<Building, GameObject> _buildingViews = new();
        private readonly Dictionary<Citizen, GameObject> _citizenViews = new();
        private float _elapsed;
        private int _placementCursor;

        private void Start()
        {
            _grid = new GridSystem(width, height);
            _simulation = new CitySimulation(_grid);

            GenerateRoadNetwork();
            GenerateTileViews();
            RunTick();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            while (_elapsed >= tickIntervalSeconds)
            {
                _elapsed -= tickIntervalSeconds;
                FillCityStep();
                RunTick();
                RefreshBuildingViews();
                RefreshCitizenViews();
            }
        }

        private void GenerateRoadNetwork()
        {
            for (var x = 0; x < width; x++)
            {
                _grid.PlaceRoad(x, 0);
                _grid.PlaceRoad(x, height - 1);

                if (x % 5 == 0)
                {
                    for (var y = 0; y < height; y++)
                    {
                        _grid.PlaceRoad(x, y);
                    }
                }
            }

            for (var y = 0; y < height; y++)
            {
                _grid.PlaceRoad(0, y);
                _grid.PlaceRoad(width - 1, y);

                if (y % 5 == 0)
                {
                    for (var x = 0; x < width; x++)
                    {
                        _grid.PlaceRoad(x, y);
                    }
                }
            }
        }

        private void FillCityStep()
        {
            var placed = 0;
            var maxTiles = width * height;
            var scanned = 0;

            while (placed < buildingsPlacedPerTick && scanned < maxTiles)
            {
                var index = _placementCursor % maxTiles;
                var x = index % width;
                var y = index / width;

                var tile = _grid.GetTile(x, y);
                if (tile != null && !tile.IsRoad && !tile.HasBuilding)
                {
                    var type = ResolveBuildingType(x, y);
                    if (_grid.PlaceBuilding(x, y, type))
                    {
                        placed++;
                    }
                }

                _placementCursor++;
                scanned++;
            }
        }

        private static BuildingType ResolveBuildingType(int x, int y)
        {
            var value = (x + y) % 10;
            return value switch
            {
                0 => BuildingType.PoliceStation,
                1 => BuildingType.FireStation,
                2 => BuildingType.Hospital,
                3 or 4 => BuildingType.Commercial,
                5 or 6 => BuildingType.Industrial,
                _ => BuildingType.Residential
            };
        }

        private void GenerateTileViews()
        {
            var root = new GameObject("GridRoot");
            root.transform.SetParent(transform, false);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tile.name = $"Tile_{x}_{y}";
                    tile.transform.SetParent(root.transform, false);
                    tile.transform.position = new Vector3(x * tileSize, 0f, y * tileSize);
                    tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    tile.transform.localScale = Vector3.one * tileSize;

                    var renderer = tile.GetComponent<Renderer>();
                    var simTile = _grid.GetTile(x, y);
                    renderer.material.color = simTile != null && simTile.IsRoad ? Color.gray : new Color(0.22f, 0.24f, 0.22f);
                }
            }
        }

        private void RefreshBuildingViews()
        {
            foreach (var placement in _grid.Placements)
            {
                var building = placement.Building;
                if (_buildingViews.TryGetValue(building, out var view))
                {
                    var heightScale = 0.5f + building.Level * 0.4f;
                    view.transform.localScale = new Vector3(tileSize * 0.8f, heightScale, tileSize * 0.8f);
                    view.transform.position = new Vector3(placement.X * tileSize, heightScale * 0.5f, placement.Y * tileSize);
                    continue;
                }

                var buildingGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                buildingGo.name = $"Building_{building.BuildingType}_{placement.X}_{placement.Y}";
                buildingGo.transform.SetParent(transform, false);

                var initialHeight = 0.5f + building.Level * 0.4f;
                buildingGo.transform.localScale = new Vector3(tileSize * 0.8f, initialHeight, tileSize * 0.8f);
                buildingGo.transform.position = new Vector3(placement.X * tileSize, initialHeight * 0.5f, placement.Y * tileSize);
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
                    view.transform.localScale = Vector3.one * (tileSize * 0.2f);
                    view.GetComponent<Renderer>().material.color = Color.white;
                    _citizenViews.Add(citizen, view);
                }

                view.SetActive(true);
                view.transform.position = new Vector3(citizen.RoadX * tileSize, 0.15f, citizen.RoadY * tileSize);
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
                BuildingType.Residential => Color.green,     // domy
                BuildingType.Industrial => Color.yellow,     // firmy
                BuildingType.Commercial => Color.blue,       // komercyjne
                BuildingType.PoliceStation => new Color(0.1f, 0.3f, 0.9f),
                BuildingType.FireStation => Color.red,
                BuildingType.Hospital => Color.white,
                _ => Color.magenta
            };
        }

        private void RunTick()
        {
            _simulation.Tick();
            Debug.Log(
                $"Tick {_simulation.TickCount} | Buildings: {_grid.Buildings.Count} | Residents: {_simulation.TotalResidents} | Jobs: {_simulation.TotalJobs} | " +
                $"Crime: {_simulation.CrimeIndex:0.0} | FireRisk: {_simulation.FireRiskIndex:0.0} | Health: {_simulation.HealthIndex:0.0} | " +
                $"Citizens: {_simulation.Citizens.Count} | Balance: {_simulation.Balance}");
        }
    }
}
