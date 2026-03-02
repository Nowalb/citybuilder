using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Root orchestrator responsible for wiring city systems and forwarding global controls (time, pause, monthly requests).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Serialized References
        public static GameManager Instance { get; private set; }

        [Header("Core")]
        [SerializeField] private TickScheduler tickScheduler;
        [SerializeField] private TerrainSystem terrainSystem;
        [SerializeField] private GridSystem gridSystem;

        [Header("Gameplay Systems")]
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private RoadManager roadManager;
        [SerializeField] private EconomySystem economySystem;
        [SerializeField] private PopulationSystem populationSystem;
        [SerializeField] private CorruptionSystem corruptionSystem;
        [SerializeField] private TechManager techManager;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private SaveLoadSystem saveLoadSystem;

        [Header("UI")]
        [SerializeField] private UIManager uiManager;

        #endregion

        #region World Settings
        [Header("World Generation")]
        [SerializeField] private int worldSeed = 12345;
        [SerializeField] private int worldWidth = 64;
        [SerializeField] private int worldHeight = 64;
        [SerializeField] private float worldScale = 0.08f;
        [SerializeField] private float cellSize = 1f;

        /// <summary>
        /// Current world seed used by terrain generation.
        /// </summary>
        public int CurrentSeed => worldSeed;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            StartGame();
        }

        /// <summary>
        /// Initializes all primary systems and starts simulation tick flow.
        /// </summary>
        public void StartGame()
        {
            terrainSystem.Generate(worldSeed, worldWidth, worldHeight, worldScale);
            gridSystem.Initialize(worldWidth, worldHeight, cellSize, terrainSystem);
            roadManager.Initialize(gridSystem);
            buildingManager.Initialize(gridSystem, roadManager, economySystem);
            populationSystem.Initialize(economySystem, buildingManager, corruptionSystem);
            eventSystem.Initialize(populationSystem, corruptionSystem, economySystem);
            saveLoadSystem.Initialize(this, buildingManager, economySystem, populationSystem, corruptionSystem, techManager, terrainSystem);
            uiManager.Initialize(this, tickScheduler, economySystem, populationSystem, corruptionSystem, buildingManager, saveLoadSystem);

            tickScheduler.OnMonthTick += OnMonthTick;
            tickScheduler.StartScheduler();
        }

        /// <summary>
        /// Pauses simulation.
        /// </summary>
        public void Pause()
        {
            tickScheduler.SetPaused(true);
        }

        /// <summary>
        /// Sets game time multiplier.
        /// </summary>
        public void SetTimeScale(float scale)
        {
            tickScheduler.SetPaused(false);
            tickScheduler.FastForward(scale);
        }

        /// <summary>
        /// Triggers monthly simulation update immediately.
        /// </summary>
        public void RequestMonthlyTick()
        {
            OnMonthTick(tickScheduler.CurrentMonth + 1);
        }

        /// <summary>
        /// Updates the world generation seed and refreshes terrain/grid.
        /// </summary>
        public void RegenerateWorld(int newSeed)
        {
            worldSeed = newSeed;
            terrainSystem.Generate(worldSeed, worldWidth, worldHeight, worldScale);
            gridSystem.Initialize(worldWidth, worldHeight, cellSize, terrainSystem);
        }

        #endregion

        #region Tick Handlers

        private void OnMonthTick(int month)
        {
            economySystem.MonthlyTick();
            populationSystem.MonthlyTick();
            corruptionSystem.MonthlyTick();
            techManager.MonthlyTick();
            eventSystem.MonthlyTick(month);
        }

        #endregion
    }
}
