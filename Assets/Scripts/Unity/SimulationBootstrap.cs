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

        private GridSystem _grid;
        private CitySimulation _simulation;
        private GameObject[,] _tileViews;
        private readonly System.Collections.Generic.Dictionary<Building, GameObject> _buildingViews = new();
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
                RefreshBuildingViews();
                RunTick();
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
            var value = (x + y) % 3;
            return value switch
            {
                0 => BuildingType.Residential,
                1 => BuildingType.Industrial,
                _ => BuildingType.Commercial
            };
        }

        private void GenerateTileViews()
        {
            _tileViews = new GameObject[width, height];
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

                    _tileViews[x, y] = tile;
                }
            }
        }

        private void RefreshBuildingViews()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var tile = _grid.GetTile(x, y);
                    if (tile == null || !tile.HasBuilding)
                    {
                        continue;
                    }

                    var building = tile.Building;
                    if (_buildingViews.TryGetValue(building, out var view))
                    {
                        view.transform.localScale = new Vector3(tileSize * 0.8f, 0.5f + building.Level * 0.4f, tileSize * 0.8f);
                        view.transform.position = new Vector3(x * tileSize, (0.5f + building.Level * 0.4f) * 0.5f, y * tileSize);
                        continue;
                    }

                    var buildingGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    buildingGo.name = $"Building_{building.BuildingType}_{x}_{y}";
                    buildingGo.transform.SetParent(transform, false);
                    buildingGo.transform.localScale = new Vector3(tileSize * 0.8f, 0.5f + building.Level * 0.4f, tileSize * 0.8f);
                    buildingGo.transform.position = new Vector3(x * tileSize, (0.5f + building.Level * 0.4f) * 0.5f, y * tileSize);

                    var renderer = buildingGo.GetComponent<Renderer>();
                    renderer.material.color = ResolveBuildingColor(building.BuildingType);

                    _buildingViews.Add(building, buildingGo);
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
                BuildingType.Service => Color.yellow,
                _ => Color.white
            };
        }

        private void RunTick()
        {
            _simulation.Tick();
            Debug.Log($"Tick {_simulation.TickCount} | Buildings: {_grid.Buildings.Count} | Residents: {_simulation.TotalResidents} | Jobs: {_simulation.TotalJobs} | Unemployed: {_simulation.Unemployed} | Income: {_simulation.Income} | Upkeep: {_simulation.Upkeep} | Balance: {_simulation.Balance}");
        }
    }
}
