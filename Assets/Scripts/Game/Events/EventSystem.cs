using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Handles periodic random city events and dispatches aggregate consequences to core systems.
    /// </summary>
    public class EventSystem : MonoBehaviour
    {
        private const float FireChance = 0.03f;
        private const float BlackoutChance = 0.02f;
        private const float ProtestChance = 0.025f;
        private const float CorruptionScandalChance = 0.02f;

        private PopulationSystem populationSystem;
        private CorruptionSystem corruptionSystem;
        private EconomySystem economySystem;

        /// <summary>
        /// Wires system dependencies.
        /// </summary>
        public void Initialize(PopulationSystem population, CorruptionSystem corruption, EconomySystem economy)
        {
            populationSystem = population;
            corruptionSystem = corruption;
            economySystem = economy;
        }

        /// <summary>
        /// Rolls event probabilities for the monthly simulation cycle.
        /// </summary>
        public void MonthlyTick(int month)
        {
            float corruptionMultiplier = corruptionSystem.GetNegativeEventMultiplier();

            if (Random.value < FireChance)
            {
                economySystem.Spend(1200);
            }

            if (Random.value < BlackoutChance)
            {
                economySystem.Spend(900);
            }

            if (Random.value < ProtestChance)
            {
                populationSystem.ApplyMoraleShock(0.35f);
            }

            if (Random.value < CorruptionScandalChance * corruptionMultiplier)
            {
                populationSystem.ApplyMoraleShock(0.5f);
                economySystem.Spend(2500);
            }
        }
    }
}
