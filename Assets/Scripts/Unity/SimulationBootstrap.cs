using System.Collections.Generic;
using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Unity adapter: drives simulation tick, world picking, and visual sync.
    /// </summary>
    public sealed class SimulationBootstrap : MonoBehaviour
    {
        [Header("Hex Grid")]
        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;
        [SerializeField] private float hexSize = 1f;

        [Header("Simulation")]
        [SerializeField] private float tickIntervalSeconds = 1f;
        [SerializeField] private int maxCitizenDots = 300;

        [Header("References")]
        [SerializeField] private HexGridRenderer hexGridRenderer;

        private GridSystem _grid;
        private CitySimulation _simulation;
        private readonly Dictionary<Building, GameObject> _buildingViews = new();
        private readonly Dictionary<Citizen, GameObject> _citizenViews = new();
        private Camera _mainCamera;
        private float _elapsed;

        public GridSystem Grid => _grid;


        private void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            hexSize = Mathf.Max(0.1f, hexSize);
            tickIntervalSeconds = Mathf.Max(0.1f, tickIntervalSeconds);
            maxCitizenDots = Mathf.Max(1, maxCitizenDots);
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
            _grid = new GridSystem(width, height);
            _simulation = new CitySimulation(_grid);

            GenerateRoadNetwork();
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
                var tile = _grid.GetTile(coord);
                hexGridRenderer.RefreshTile(coord, tile != null && tile.IsRoad);
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

        private void GenerateRoadNetwork()
        {
            for (var q = 0; q < width; q++)
            {
                PlaceRoad(new HexCoord(q, 0));
                PlaceRoad(new HexCoord(q, height - 1));
            }

            for (var r = 0; r < height; r++)
            {
                PlaceRoad(new HexCoord(0, r));
                PlaceRoad(new HexCoord(width - 1, r));
            }

            for (var q = 0; q < width; q += 5)
            {
                for (var r = 0; r < height; r++)
                {
                    PlaceRoad(new HexCoord(q, r));
                }
            }

            for (var r = 0; r < height; r += 5)
            {
                for (var q = 0; q < width; q++)
                {
                    PlaceRoad(new HexCoord(q, r));
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
                $"Tick {_simulation.TickCount} | Buildings: {_grid.Buildings.Count} | Residents: {_simulation.TotalResidents} | Jobs: {_simulation.TotalJobs} | " +
                $"Crime: {_simulation.CrimeIndex:0.0} | FireRisk: {_simulation.FireRiskIndex:0.0} | Health: {_simulation.HealthIndex:0.0} | " +
                $"Citizens: {_simulation.Citizens.Count} | Balance: {_simulation.Balance}");
#endif
        }
    }
}
