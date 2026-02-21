using CityBuilder.Simulation;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CityBuilder.Unity
{
    /// <summary>
    /// Handles build tool UI and click-to-place actions on the hex grid.
    /// </summary>
    public sealed class PlacementController : MonoBehaviour
    {
        public enum BuildTool
        {
            Road,
            Residential,
            Industrial,
            Commercial,
            PoliceStation,
            FireStation,
            Hospital
        }

        [SerializeField] private SimulationBootstrap bootstrap;
        [SerializeField] private BuildTool activeTool = BuildTool.Road;

        private void Update()
        {
            if (!IsPlacePressed() || bootstrap == null)
            {
                return;
            }

            if (!bootstrap.TryPickHex(GetPointerScreenPosition(), out var coord))
            {
                return;
            }

            var placed = activeTool == BuildTool.Road
                ? bootstrap.PlaceRoad(coord)
                : bootstrap.PlaceBuilding(coord, ToBuildingType(activeTool));

            if (placed)
            {
                bootstrap.RefreshViews();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 760, 110), "Build Menu", GUI.skin.window);
            GUILayout.Label($"Active tool: {activeTool}");
            GUILayout.BeginHorizontal();

            DrawToolButton("Road", BuildTool.Road);
            DrawToolButton("Residential", BuildTool.Residential);
            DrawToolButton("Industrial", BuildTool.Industrial);
            DrawToolButton("Commercial", BuildTool.Commercial);
            DrawToolButton("Police", BuildTool.PoliceStation);
            DrawToolButton("Fire", BuildTool.FireStation);
            DrawToolButton("Hospital", BuildTool.Hospital);

            GUILayout.EndHorizontal();
            GUILayout.Label("LPM: place selected tool on hex tile. Buildings require adjacent road.");
            GUILayout.EndArea();
        }

        private void DrawToolButton(string label, BuildTool tool)
        {
            var prev = GUI.backgroundColor;
            if (activeTool == tool)
            {
                GUI.backgroundColor = Color.cyan;
            }

            if (GUILayout.Button(label, GUILayout.Height(34)))
            {
                activeTool = tool;
            }

            GUI.backgroundColor = prev;
        }

        private static BuildingType ToBuildingType(BuildTool tool)
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

#if ENABLE_INPUT_SYSTEM
        private static bool IsPlacePressed() => Mouse.current?.leftButton.wasPressedThisFrame ?? false;
        private static Vector2 GetPointerScreenPosition() => Mouse.current?.position.ReadValue() ?? Vector2.zero;
#else
        private static bool IsPlacePressed() => Input.GetMouseButtonDown(0);
        private static Vector2 GetPointerScreenPosition() => Input.mousePosition;
#endif
    }
}
