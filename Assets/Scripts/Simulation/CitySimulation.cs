using System;
using System.Collections.Generic;

namespace CityBuilder.Simulation
{
    /// <summary>
    /// Pure C# simulation service. No Unity dependencies.
    /// </summary>
    public sealed class CitySimulation
    {
        private readonly GridSystem _gridSystem;
        private readonly Random _random;

        public int TickCount { get; private set; }
        public int TotalResidents { get; private set; }
        public int TotalJobs { get; private set; }
        public int Unemployed { get; private set; }
        public int Income { get; private set; }
        public int Upkeep { get; private set; }
        public int Balance { get; private set; }

        public CitySimulation(GridSystem gridSystem, int? randomSeed = null)
        {
            _gridSystem = gridSystem ?? throw new ArgumentNullException(nameof(gridSystem));
            _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
        }

        public void Tick()
        {
            TickCount++;
            CalculateStats();

            if (Unemployed == 0)
            {
                UpgradeRandomResidential();
                CalculateStats();
            }
        }

        private void CalculateStats()
        {
            TotalResidents = 0;
            TotalJobs = 0;
            Upkeep = 0;

            foreach (var building in _gridSystem.Buildings)
            {
                TotalResidents += building.Residents;
                TotalJobs += building.Jobs;
                Upkeep += building.UpkeepCost;
            }

            Unemployed = Math.Max(0, TotalResidents - TotalJobs);
            Income = TotalResidents * 2;
            Balance = Income - Upkeep;
        }

        private void UpgradeRandomResidential()
        {
            var candidates = new List<Building>();

            foreach (var building in _gridSystem.Buildings)
            {
                if (building.BuildingType == BuildingType.Residential)
                {
                    candidates.Add(building);
                }
            }

            if (candidates.Count == 0)
            {
                return;
            }

            candidates[_random.Next(candidates.Count)].Upgrade();
        }
    }
}
