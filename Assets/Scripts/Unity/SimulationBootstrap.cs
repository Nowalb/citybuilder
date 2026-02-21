using System.Collections.Generic;
using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Unity adapter: drives simulation, rendering and manual city-builder placement tools.
    /// </summary>
    public sealed class SimulationBootstrap : MonoBehaviour
    {
        private enum BuildTool
        {
            Road,
            Residential,
            Industrial,
            Commercial,
            PoliceStation,
            FireStation,
            Hospital
        }

        [Header("Grid")]
        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;
        [SerializeField] private float tileSize = 1f;

        [Header("Simulation")]
        [SerializeField] private float tickIntervalSeconds = 1f;

        [Header("Citizens")]
        [SerializeField] private int maxCitizenDots = 300;

        private GridSystem _grid;
        private CitySimulation _simulation;
        private readonly Dictionary<Building, GameObject> _buildingViews = new();
        private readonly Dictionary<Citizen, GameObject> _citizenViews = new();
        private readonly Dictionary<GameObject, Vector2Int> _tilePositionsByView = new();

        private BuildTool _activeTool = BuildTool.Road;
        private float _elapsed;

        private void Start()
        {
            _grid = new GridSystem(width, height);
            _simulation = new CitySimulation(_grid);

            GenerateRoadNetwork();
            GenerateTileViews();
            RefreshBuildingViews();
            RefreshCitizenViews();
            RunTick();
        }

        private void Update()
        {
            HandlePlacementInput();

            _elapsed += Time.deltaTime;
            while (_elapsed >= tickIntervalSeconds)
            {
                _elapsed -= tickIntervalSeconds;
                RunTick();
                RefreshBuildingViews();
                RefreshCitizenViews();
            }
        }

        private void HandlePlacementInput()
        {
            if (!IsPlacePressed())
            {
                return;
            }

            if (!TryGetHoveredTilePosition(out var position))
            {
                return;
            }

            var success = _activeTool == BuildTool.Road
                ? _grid.PlaceRoad(position.x, position.y)
                : _grid.PlaceBuilding(position.x, position.y, ConvertToolToBuildingType(_activeTool));

            if (!success)
            {
                return;
            }

            UpdateTileColor(position.x, position.y);
            RefreshBuildingViews();
        }

        private bool TryGetHoveredTilePosition(out Vector2Int tilePosition)
        {
            tilePosition = default;
            var ray = Camera.main.ScreenPointToRay(GetPointerScreenPosition());

            if (!Physics.Raycast(ray, out var hit, 500f))
            {
                return false;
            }

            return _tilePositionsByView.TryGetValue(hit.collider.gameObject, out tilePosition);
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

                    _tilePositionsByView.Add(tile, new Vector2Int(x, y));
                    UpdateTileColor(x, y);
                }
            }
        }

        private void UpdateTileColor(int x, int y)
        {
            foreach (var kv in _tilePositionsByView)
            {
                if (kv.Value.x != x || kv.Value.y != y)
                {
                    continue;
                }

                var tile = _grid.GetTile(x, y);
                var renderer = kv.Key.GetComponent<Renderer>();
                renderer.material.color = tile != null && tile.IsRoad ? Color.gray : new Color(0.22f, 0.24f, 0.22f);
                return;
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

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 610, 90), "Build Menu", GUI.skin.window);
            GUILayout.Label($"Active tool: {_activeTool}");
            GUILayout.BeginHorizontal();

            DrawToolButton("Road", BuildTool.Road);
            DrawToolButton("Residential", BuildTool.Residential);
            DrawToolButton("Industrial", BuildTool.Industrial);
            DrawToolButton("Commercial", BuildTool.Commercial);
            DrawToolButton("Police", BuildTool.PoliceStation);
            DrawToolButton("Fire", BuildTool.FireStation);
            DrawToolButton("Hospital", BuildTool.Hospital);

            GUILayout.EndHorizontal();
            GUILayout.Label("LPM: place selected tool on tile. Buildings require adjacent road.");
            GUILayout.EndArea();
        }

        private void DrawToolButton(string label, BuildTool tool)
        {
            var previousColor = GUI.backgroundColor;
            if (_activeTool == tool)
            {
                GUI.backgroundColor = Color.cyan;
            }

            if (GUILayout.Button(label, GUILayout.Height(30)))
            {
                _activeTool = tool;
            }

            GUI.backgroundColor = previousColor;
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

        private static BuildingType ConvertToolToBuildingType(BuildTool tool)
        {
            return tool switch
            {
                BuildTool.Residential => BuildingType.Residential,
                BuildTool.Industrial => BuildingType.Industrial,
                BuildTool.Commercial => BuildingType.Commercial,
                BuildTool.PoliceStation => BuildingType.PoliceStation,
                BuildTool.FireStation => BuildingType.FireStation,
                BuildTool.Hospital => BuildingType.Hospital,
                _ => BuildingType.Empty
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

#if ENABLE_INPUT_SYSTEM
        private static bool IsPlacePressed()
        {
            return UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame ?? false;
        }

        private static Vector2 GetPointerScreenPosition()
        {
            return UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }
#else
        private static bool IsPlacePressed()
        {
            return Input.GetMouseButtonDown(0);
        }

        private static Vector2 GetPointerScreenPosition()
        {
            return Input.mousePosition;
        }
#endif
    }
}
