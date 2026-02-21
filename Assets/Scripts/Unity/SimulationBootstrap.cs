using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Unity adapter: initializes simulation and creates visual GameObjects.
    /// </summary>
    public sealed class SimulationBootstrap : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;
        [SerializeField] private float tileSize = 1f;

        [Header("Simulation")]
        [SerializeField] private float tickIntervalSeconds = 1f;

        [Header("View")]
        [SerializeField] private Material tileMaterial;
        [SerializeField] private Material residentialMaterial;
        [SerializeField] private Material commercialMaterial;
        [SerializeField] private Material industrialMaterial;
        [SerializeField] private Material serviceMaterial;

        private GridSystem _grid;
        private CitySimulation _simulation;
        private GameObject[,] _tileViews;
        private readonly System.Collections.Generic.Dictionary<Building, GameObject> _buildingViews = new();
        private float _elapsed;

        private void Start()
        {
            _grid = new GridSystem(width, height);
            _simulation = new CitySimulation(_grid);

            GenerateTileViews();
            SeedCity();
            RefreshBuildingViews();
            RunTick();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            while (_elapsed >= tickIntervalSeconds)
            {
                _elapsed -= tickIntervalSeconds;
                RunTick();
                RefreshBuildingViews();
            }
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

                    if (tileMaterial != null)
                    {
                        tile.GetComponent<Renderer>().material = tileMaterial;
                    }

                    _tileViews[x, y] = tile;
                }
            }
        }

        private void SeedCity()
        {
            _grid.PlaceBuilding(10, 10, BuildingType.Residential);
            _grid.PlaceBuilding(10, 11, BuildingType.Residential);
            _grid.PlaceBuilding(12, 10, BuildingType.Commercial);
            _grid.PlaceBuilding(13, 10, BuildingType.Industrial);
            _grid.PlaceBuilding(14, 10, BuildingType.Service);
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
                        continue;
                    }

                    var buildingGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    buildingGo.name = $"Building_{building.BuildingType}_{x}_{y}";
                    buildingGo.transform.SetParent(transform, false);
                    buildingGo.transform.position = new Vector3(x * tileSize, 0.25f, y * tileSize);
                    buildingGo.transform.localScale = new Vector3(tileSize * 0.8f, 0.5f + building.Level * 0.4f, tileSize * 0.8f);

                    var renderer = buildingGo.GetComponent<Renderer>();
                    renderer.material = ResolveBuildingMaterial(building.BuildingType, renderer.material);

                    _buildingViews.Add(building, buildingGo);
                }
            }
        }

        private Material ResolveBuildingMaterial(BuildingType type, Material fallback)
        {
            return type switch
            {
                BuildingType.Residential => residentialMaterial != null ? residentialMaterial : fallback,
                BuildingType.Commercial => commercialMaterial != null ? commercialMaterial : fallback,
                BuildingType.Industrial => industrialMaterial != null ? industrialMaterial : fallback,
                BuildingType.Service => serviceMaterial != null ? serviceMaterial : fallback,
                _ => fallback
            };
        }

        private void RunTick()
        {
            _simulation.Tick();
            Debug.Log($"Tick {_simulation.TickCount} | Residents: {_simulation.TotalResidents} | Jobs: {_simulation.TotalJobs} | Unemployed: {_simulation.Unemployed} | Income: {_simulation.Income} | Upkeep: {_simulation.Upkeep} | Balance: {_simulation.Balance}");
        }
    }
}
