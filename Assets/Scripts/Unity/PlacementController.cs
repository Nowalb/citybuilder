using System.Collections.Generic;
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
    /// Manual placement controller with click-and-drag painting across hexes.
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

        private static readonly Rect BuildMenuRect = new(10f, 0f, 760f, 92f);

        private bool _isDragging;
        private HexCoord _lastDragCoord;

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
            if (bootstrap == null)
            {
                return;
            }

            if (IsPointerOverUi())
            {
                _isDragging = false;
                return;
            }

            if (IsPlaceStarted())
            {
                if (TryPick(out var start))
                {
                    _isDragging = true;
                    _lastDragCoord = start;
                    TryPlace(start);
                }
            }

            if (_isDragging && IsPlaceHeld())
            {
                if (TryPick(out var current))
                {
                    PaintDragPath(_lastDragCoord, current);
                    _lastDragCoord = current;
                }
            }

            if (_isDragging && IsPlaceReleased())
            {
                _isDragging = false;
            }
        }

        private void OnGUI()
        {
            var rect = new Rect(BuildMenuRect.x, Screen.height - 105f, BuildMenuRect.width, BuildMenuRect.height);
            GUILayout.BeginArea(rect, "Build", GUI.skin.window);
            GUILayout.BeginHorizontal();
            DrawToolButton("Road", BuildTool.Road);
            DrawToolButton("Residential", BuildTool.Residential);
            DrawToolButton("Industrial", BuildTool.Industrial);
            DrawToolButton("Commercial", BuildTool.Commercial);
            DrawToolButton("Police", BuildTool.PoliceStation);
            DrawToolButton("Fire", BuildTool.FireStation);
            DrawToolButton("Hospital", BuildTool.Hospital);
            GUILayout.EndHorizontal();
            GUILayout.Label("Click and drag to paint across hexes.");
            GUILayout.EndArea();
        }

        private void DrawToolButton(string label, BuildTool tool)
        {
            var prev = GUI.backgroundColor;
            if (activeTool == tool)
            {
                GUI.backgroundColor = Color.cyan;
            }

            if (GUILayout.Button(label, GUILayout.Height(30), GUILayout.MinWidth(95)))
            {
                activeTool = tool;
            }

            GUI.backgroundColor = prev;
        }

        private void PaintDragPath(HexCoord from, HexCoord to)
        {
            foreach (var coord in HexLine(from, to))
            {
                TryPlace(coord);
            }

            bootstrap.RefreshViews();
        }

        private void TryPlace(HexCoord coord)
        {
            if (activeTool == BuildTool.Road)
            {
                bootstrap.PlaceRoad(coord);
                return;
            }

            bootstrap.PlaceBuilding(coord, ToBuildingType(activeTool));
        }

        private bool TryPick(out HexCoord coord)
        {
            return bootstrap.TryPickHex(GetPointerScreenPosition(), out coord);
        }

        private bool IsPointerOverUi()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            var pos = GetPointerScreenPosition();
            var uiRect = new Rect(BuildMenuRect.x, Screen.height - 105f, BuildMenuRect.width, BuildMenuRect.height);
            var converted = new Vector2(pos.x, Screen.height - pos.y);
            return uiRect.Contains(converted);
        }

        private static IEnumerable<HexCoord> HexLine(HexCoord a, HexCoord b)
        {
            var n = HexDistance(a, b);
            if (n == 0)
            {
                yield return a;
                yield break;
            }

            for (var i = 0; i <= n; i++)
            {
                var t = i / (float)n;
                var aq = a.Q + (b.Q - a.Q) * t;
                var ar = a.R + (b.R - a.R) * t;
                yield return HexGridMath.AxialRound(aq, ar);
            }
        }

        private static int HexDistance(HexCoord a, HexCoord b)
        {
            var dq = a.Q - b.Q;
            var dr = a.R - b.R;
            var ds = (-a.Q - a.R) - (-b.Q - b.R);
            return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(ds)) / 2;
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
        private static bool IsPlaceStarted() => Mouse.current?.leftButton.wasPressedThisFrame ?? false;
        private static bool IsPlaceHeld() => Mouse.current?.leftButton.isPressed ?? false;
        private static bool IsPlaceReleased() => Mouse.current?.leftButton.wasReleasedThisFrame ?? false;
        private static Vector2 GetPointerScreenPosition() => Mouse.current?.position.ReadValue() ?? Vector2.zero;
#else
        private static bool IsPlaceStarted() => Input.GetMouseButtonDown(0);
        private static bool IsPlaceHeld() => Input.GetMouseButton(0);
        private static bool IsPlaceReleased() => Input.GetMouseButtonUp(0);
        private static Vector2 GetPointerScreenPosition() => Input.mousePosition;
#endif
    }
}
