using System.Collections.Generic;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Aggregated city finance simulation with taxes, upkeep, debt interest and inflation.
    /// </summary>
    public class EconomySystem : MonoBehaviour
    {
        #region Constants and Config
        public enum PopulationClass { Low, Mid, High }

        [SerializeField] private long startingBudget = 100_000;
        [SerializeField] private float annualDebtInterest = 0.12f;
        [SerializeField] private float inflation = 0.02f;

        private readonly Dictionary<PopulationClass, float> taxRates = new Dictionary<PopulationClass, float>
        {
            { PopulationClass.Low, 0.10f },
            { PopulationClass.Mid, 0.12f },
            { PopulationClass.High, 0.16f }
        };

        private readonly Dictionary<PopulationClass, float> baseIncomePerPerson = new Dictionary<PopulationClass, float>
        {
            { PopulationClass.Low, 40f },
            { PopulationClass.Mid, 75f },
            { PopulationClass.High, 120f }
        };

        #endregion

        #region Runtime State

        private PopulationSystem populationSystem;
        private BuildingManager buildingManager;

        public long Budget { get; private set; }
        public long MonthlyIncome { get; private set; }
        public long MonthlyExpenses { get; private set; }
        public long Debt { get; private set; }
        public float Inflation => inflation;

        #endregion

        #region Lifecycle and API

        private void Awake()
        {
            Budget = startingBudget;
        }

        /// <summary>
        /// Connects dependent systems used in monthly calculations.
        /// </summary>
        public void Bind(PopulationSystem population, BuildingManager buildings)
        {
            populationSystem = population;
            buildingManager = buildings;
        }

        /// <summary>
        /// Adds direct income.
        /// </summary>
        public void AddIncome(long amount)
        {
            Budget += amount;
            MonthlyIncome += amount;
        }

        /// <summary>
        /// Spends funds and creates debt if budget is insufficient.
        /// </summary>
        public bool Spend(long amount)
        {
            if (amount <= 0) return true;

            Budget -= amount;
            if (Budget < 0)
            {
                Debt += -Budget;
                Budget = 0;
            }

            MonthlyExpenses += amount;
            return true;
        }

        /// <summary>
        /// Sets tax rate by social class.
        /// </summary>
        public void SetTaxRateForClass(PopulationClass classType, float rate)
        {
            taxRates[classType] = Mathf.Clamp(rate, 0f, 0.5f);
        }

        /// <summary>
        /// Applies one monthly finance cycle.
        /// taxRevenue = Σ(pop[class] * baseIncome[class] * taxRate[class])
        /// upkeepTotal = Σ(buildings.upkeepPerMonth)
        /// debtInterest = debt * annualInterest/12
        /// </summary>
        public void MonthlyTick()
        {
            MonthlyIncome = 0;
            MonthlyExpenses = 0;

            long taxRevenue = 0;
            if (populationSystem != null)
            {
                taxRevenue += CalcTax(populationSystem.LowPopulation, PopulationClass.Low);
                taxRevenue += CalcTax(populationSystem.MidPopulation, PopulationClass.Mid);
                taxRevenue += CalcTax(populationSystem.HighPopulation, PopulationClass.High);
            }

            long upkeepTotal = buildingManager?.GetTotalUpkeep() ?? 0;
            long debtInterest = Mathf.RoundToInt(Debt * (annualDebtInterest / 12f));

            taxRevenue = Mathf.RoundToInt(taxRevenue * (1f + inflation));

            AddIncome(taxRevenue);
            Spend(upkeepTotal + debtInterest);

            if (Debt > 0 && Budget > 0)
            {
                long payback = Mathf.Min(Budget, Debt / 10 + 1);
                Budget -= payback;
                Debt -= payback;
            }
        }

        /// <summary>
        /// Assigns budget/debt values during save-game restore.
        /// </summary>
        public void Restore(long budget, long debt)
        {
            Budget = budget;
            Debt = debt;
        }

        #endregion

        #region Internals

        private long CalcTax(int population, PopulationClass cls)
        {
            float value = population * baseIncomePerPerson[cls] * taxRates[cls];
            return Mathf.RoundToInt(value);
        }

        #endregion
    }
}
