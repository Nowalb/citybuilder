using CityBuilder.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace CityBuilder.Unity
{
    /// <summary>
    /// Optional manual placement controller kept for debugging.
    /// Auto-growth works without this component.
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

        private void Awake()
        {
            EnsureEventSystemExists();

            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<SimulationBootstrap>();
            }
        }

        private void Update()
        {
            if (!IsPlacePressed() || bootstrap == null)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
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

        private static void EnsureEventSystemExists()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
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
