using CityBuilder.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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

        private Canvas _canvas;

        private void Awake()
        {
            EnsureEventSystemExists();

            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<SimulationBootstrap>();
            }

            EnsureBuildCanvas();
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

        private void EnsureBuildCanvas()
        {
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null)
            {
                var canvasGo = new GameObject("BuildCanvas", typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler));
                _canvas = canvasGo.GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }

            var panel = new GameObject("BuildPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_canvas.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = new Vector2(0f, 120f);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            panel.GetComponent<Image>().raycastTarget = true;

            var tools = new[]
            {
                ("Road", BuildTool.Road),
                ("Residential", BuildTool.Residential),
                ("Industrial", BuildTool.Industrial),
                ("Commercial", BuildTool.Commercial),
                ("Police", BuildTool.PoliceStation),
                ("Fire", BuildTool.FireStation),
                ("Hospital", BuildTool.Hospital)
            };

            for (var i = 0; i < tools.Length; i++)
            {
                CreateToolButton(panel.transform, tools[i].Item1, tools[i].Item2, i, tools.Length);
            }
        }

        private void CreateToolButton(Transform parent, string label, BuildTool tool, int index, int total)
        {
            var buttonGo = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            var rect = buttonGo.GetComponent<RectTransform>();
            var width = 1f / total;
            rect.anchorMin = new Vector2(index * width, 0f);
            rect.anchorMax = new Vector2((index + 1) * width, 1f);
            rect.offsetMin = new Vector2(6f, 8f);
            rect.offsetMax = new Vector2(-6f, -8f);

            var image = buttonGo.GetComponent<Image>();
            image.color = tool == activeTool ? Color.cyan : new Color(0.16f, 0.16f, 0.16f, 1f);
            image.raycastTarget = true;

            var button = buttonGo.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() =>
            {
                activeTool = tool;
                RefreshButtonHighlights(parent);
            });

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private void RefreshButtonHighlights(Transform panel)
        {
            foreach (Transform child in panel)
            {
                if (!child.name.StartsWith("Btn_"))
                {
                    continue;
                }

                var image = child.GetComponent<Image>();
                if (image == null)
                {
                    continue;
                }

                var toolName = child.name[4..];
                var isActive =
                    (toolName == "Road" && activeTool == BuildTool.Road) ||
                    (toolName == "Residential" && activeTool == BuildTool.Residential) ||
                    (toolName == "Industrial" && activeTool == BuildTool.Industrial) ||
                    (toolName == "Commercial" && activeTool == BuildTool.Commercial) ||
                    (toolName == "Police" && activeTool == BuildTool.PoliceStation) ||
                    (toolName == "Fire" && activeTool == BuildTool.FireStation) ||
                    (toolName == "Hospital" && activeTool == BuildTool.Hospital);

                image.color = isActive ? Color.cyan : new Color(0.16f, 0.16f, 0.16f, 1f);
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
