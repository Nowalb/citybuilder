using UnityEngine;
using UnityEngine.UI;

namespace CityCore
{
    /// <summary>
    /// Minimal HUD controller for budget/time/population display and basic simulation controls.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private Text budgetLabel;
        [SerializeField] private Text dateLabel;
        [SerializeField] private Text populationLabel;
        [SerializeField] private Text corruptionLabel;
        [SerializeField] private Text tutorialLabel;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speed1Button;
        [SerializeField] private Button speed2Button;
        [SerializeField] private Button speed4Button;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button exportSeedButton;

        private GameManager gameManager;
        private TickScheduler tickScheduler;
        private EconomySystem economySystem;
        private PopulationSystem populationSystem;
        private CorruptionSystem corruptionSystem;
        private BuildingManager buildingManager;
        private SaveLoadSystem saveLoadSystem;

        /// <summary>
        /// Initializes UI references and button handlers.
        /// </summary>
        public void Initialize(
            GameManager game,
            TickScheduler scheduler,
            EconomySystem economy,
            PopulationSystem population,
            CorruptionSystem corruption,
            BuildingManager buildings,
            SaveLoadSystem saveLoad)
        {
            gameManager = game;
            tickScheduler = scheduler;
            economySystem = economy;
            populationSystem = population;
            corruptionSystem = corruption;
            buildingManager = buildings;
            saveLoadSystem = saveLoad;

            pauseButton?.onClick.AddListener(gameManager.Pause);
            speed1Button?.onClick.AddListener(() => gameManager.SetTimeScale(1f));
            speed2Button?.onClick.AddListener(() => gameManager.SetTimeScale(2f));
            speed4Button?.onClick.AddListener(() => gameManager.SetTimeScale(4f));
            saveButton?.onClick.AddListener(() => saveLoadSystem.Save());
            loadButton?.onClick.AddListener(() => saveLoadSystem.Load());
            exportSeedButton?.onClick.AddListener(ExportSeed);

            if (tickScheduler != null)
            {
                tickScheduler.OnDayTick += _ => RefreshHud();
            }

            ShowTutorial();
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (economySystem != null) budgetLabel.text = $"Budget: {economySystem.Budget} | Debt: {economySystem.Debt}";
            if (tickScheduler != null)
            {
                int day = Mathf.Max(1, tickScheduler.CurrentDay % TickScheduler.DaysPerMonth);
                int month = Mathf.Max(1, tickScheduler.CurrentMonth % 12);
                int year = 1 + tickScheduler.CurrentMonth / 12;
                dateLabel.text = $"Day {day}, Month {month}, Year {year}";
            }

            if (populationSystem != null)
            {
                populationLabel.text = $"Pop L/M/H: {populationSystem.LowPopulation}/{populationSystem.MidPopulation}/{populationSystem.HighPopulation}";
            }

            if (corruptionSystem != null)
            {
                corruptionLabel.text = $"Corruption: {corruptionSystem.CorruptionLevel:0.0}";
            }
        }

        private void ShowTutorial()
        {
            if (tutorialLabel == null) return;
            tutorialLabel.text = "Tutorial: 1) Wybierz budynek 2) Kliknij teren aby postawić 3) Buduj drogi 4) Kontroluj dług i korupcję.";
        }

        private void ExportSeed()
        {
            GUIUtility.systemCopyBuffer = gameManager.CurrentSeed.ToString();
            Debug.Log($"Seed copied to clipboard: {gameManager.CurrentSeed}");
        }
    }
}
