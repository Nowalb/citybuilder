using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Aggregate population simulation with three social classes and monthly migration logic.
    /// </summary>
    public class PopulationSystem : MonoBehaviour
    {
        [SerializeField] private int lowPopulation = 120;
        [SerializeField] private int midPopulation = 40;
        [SerializeField] private int highPopulation = 10;

        private EconomySystem economySystem;
        private BuildingManager buildingManager;
        private CorruptionSystem corruptionSystem;

        public int LowPopulation => lowPopulation;
        public int MidPopulation => midPopulation;
        public int HighPopulation => highPopulation;
        public int TotalPopulation => lowPopulation + midPopulation + highPopulation;

        /// <summary>
        /// Injects dependencies for monthly demand/migration updates.
        /// </summary>
        public void Initialize(EconomySystem economy, BuildingManager buildings, CorruptionSystem corruption)
        {
            economySystem = economy;
            buildingManager = buildings;
            corruptionSystem = corruption;
            economySystem.Bind(this, buildingManager);
        }

        /// <summary>
        /// Applies monthly migration based on housing, economy and corruption pressure.
        /// </summary>
        public void MonthlyTick()
        {
            int capacity = Mathf.Max(0, buildingManager.GetResidentialCapacity());
            int freeHousing = capacity - TotalPopulation;

            float economyFactor = Mathf.Clamp01(1f - (float)economySystem.Debt / 200_000f);
            float corruptionPenalty = corruptionSystem.CorruptionLevel / 100f;
            float satisfaction = Mathf.Clamp01(0.4f + economyFactor * 0.5f - corruptionPenalty * 0.4f);

            int migration = Mathf.RoundToInt(freeHousing * (satisfaction - 0.35f) * 0.2f);
            lowPopulation = Mathf.Max(0, lowPopulation + migration);
            midPopulation = Mathf.Max(0, midPopulation + Mathf.RoundToInt(migration * 0.35f));
            highPopulation = Mathf.Max(0, highPopulation + Mathf.RoundToInt(migration * 0.12f));

            ClampToCapacity(capacity);
        }

        /// <summary>
        /// Applies a temporary morale penalty, triggering net migration losses.
        /// </summary>
        public void ApplyMoraleShock(float severity)
        {
            int loss = Mathf.RoundToInt(TotalPopulation * Mathf.Clamp01(severity) * 0.05f);
            lowPopulation = Mathf.Max(0, lowPopulation - loss);
            midPopulation = Mathf.Max(0, midPopulation - Mathf.RoundToInt(loss * 0.35f));
            highPopulation = Mathf.Max(0, highPopulation - Mathf.RoundToInt(loss * 0.2f));
        }

        /// <summary>
        /// Restores population buckets from saved state.
        /// </summary>
        public void Restore(int low, int mid, int high)
        {
            lowPopulation = low;
            midPopulation = mid;
            highPopulation = high;
        }

        private void ClampToCapacity(int capacity)
        {
            int overflow = TotalPopulation - capacity;
            if (overflow <= 0) return;
            lowPopulation = Mathf.Max(0, lowPopulation - overflow);
        }
    }
}
