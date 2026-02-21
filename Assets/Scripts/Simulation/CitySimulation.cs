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
        private readonly List<Citizen> _citizens;

        public int TickCount { get; private set; }
        public int TotalResidents { get; private set; }
        public int TotalJobs { get; private set; }
        public int Unemployed { get; private set; }
        public int Income { get; private set; }
        public int Upkeep { get; private set; }
        public int Balance { get; private set; }

        public float CrimeIndex { get; private set; }
        public float FireRiskIndex { get; private set; }
        public float HealthIndex { get; private set; }

        public IReadOnlyList<Citizen> Citizens => _citizens;

        public CitySimulation(GridSystem gridSystem, int? randomSeed = null)
        {
            _gridSystem = gridSystem ?? throw new ArgumentNullException(nameof(gridSystem));
            _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            _citizens = new List<Citizen>();
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

            SyncCitizensToPopulation();
            MoveCitizens();
        }

        private void CalculateStats()
        {
            var policeCount = 0;
            var fireStationCount = 0;
            var hospitalCount = 0;
            var industrialCount = 0;
            var commercialCount = 0;

            TotalResidents = 0;
            TotalJobs = 0;
            Upkeep = 0;

            foreach (var building in _gridSystem.Buildings)
            {
                TotalResidents += building.Residents;
                TotalJobs += building.Jobs;
                Upkeep += building.UpkeepCost;

                switch (building.BuildingType)
                {
                    case BuildingType.PoliceStation:
                        policeCount++;
                        break;
                    case BuildingType.FireStation:
                        fireStationCount++;
                        break;
                    case BuildingType.Hospital:
                        hospitalCount++;
                        break;
                    case BuildingType.Industrial:
                        industrialCount++;
                        break;
                    case BuildingType.Commercial:
                        commercialCount++;
                        break;
                }
            }

            Unemployed = Math.Max(0, TotalResidents - TotalJobs);
            Income = TotalResidents * 2;
            Balance = Income - Upkeep;

            CrimeIndex = Clamp01((TotalResidents / 350f) - (policeCount * 0.20f)) * 100f;
            FireRiskIndex = Clamp01((industrialCount * 0.06f) + (commercialCount * 0.03f) + (TotalResidents / 1000f) - (fireStationCount * 0.20f)) * 100f;
            HealthIndex = Clamp01(0.45f + (hospitalCount * 0.18f) - (CrimeIndex / 200f) - (FireRiskIndex / 250f)) * 100f;
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

        private void SyncCitizensToPopulation()
        {
            var targetCount = Math.Min(TotalResidents, 400);
            if (_citizens.Count >= targetCount)
            {
                return;
            }

            var residences = new List<GridSystem.BuildingPlacement>();
            var workplaces = new List<GridSystem.BuildingPlacement>();
            var shops = new List<GridSystem.BuildingPlacement>();

            foreach (var placement in _gridSystem.Placements)
            {
                switch (placement.Building.BuildingType)
                {
                    case BuildingType.Residential:
                        residences.Add(placement);
                        break;
                    case BuildingType.Industrial:
                    case BuildingType.PoliceStation:
                    case BuildingType.FireStation:
                    case BuildingType.Hospital:
                        workplaces.Add(placement);
                        break;
                    case BuildingType.Commercial:
                        shops.Add(placement);
                        break;
                }
            }

            if (residences.Count == 0 || workplaces.Count == 0 || shops.Count == 0)
            {
                return;
            }

            while (_citizens.Count < targetCount)
            {
                var home = residences[_random.Next(residences.Count)];
                var work = workplaces[_random.Next(workplaces.Count)];
                var shop = shops[_random.Next(shops.Count)];

                if (!_gridSystem.TryGetAdjacentRoad(home.X, home.Y, out var roadX, out var roadY))
                {
                    break;
                }

                var citizen = new Citizen(home.X, home.Y, work.X, work.Y, shop.X, shop.Y, roadX, roadY);
                SetCitizenTarget(citizen);
                _citizens.Add(citizen);
            }
        }

        private void MoveCitizens()
        {
            foreach (var citizen in _citizens)
            {
                if (citizen.IsAtTarget())
                {
                    citizen.AdvancePhase();
                    SetCitizenTarget(citizen);
                }

                var next = CalculateNextRoadStep(citizen.RoadX, citizen.RoadY, citizen);
                citizen.Step(next.x, next.y);
            }
        }

        private void SetCitizenTarget(Citizen citizen)
        {
            var destination = citizen.CurrentPhase switch
            {
                0 => (citizen.WorkX, citizen.WorkY),
                1 => (citizen.ShopX, citizen.ShopY),
                _ => (citizen.HomeX, citizen.HomeY)
            };

            if (_gridSystem.TryGetAdjacentRoad(destination.Item1, destination.Item2, out var roadX, out var roadY))
            {
                citizen.SetTargetRoad(roadX, roadY);
            }
        }

        private (int x, int y) CalculateNextRoadStep(int currentX, int currentY, Citizen citizen)
        {
            // Move along roads only. Works well with the orthogonal road network.
            if (TryStepToward(currentX, currentY, currentX + Math.Sign(citizen.TargetRoadX - currentX), currentY, out var nextX, out var nextY))
            {
                return (nextX, nextY);
            }

            if (TryStepToward(currentX, currentY, currentX, currentY + Math.Sign(citizen.TargetRoadY - currentY), out nextX, out nextY))
            {
                return (nextX, nextY);
            }

            if (TryStepToward(currentX, currentY, currentX + 1, currentY, out nextX, out nextY) ||
                TryStepToward(currentX, currentY, currentX - 1, currentY, out nextX, out nextY) ||
                TryStepToward(currentX, currentY, currentX, currentY + 1, out nextX, out nextY) ||
                TryStepToward(currentX, currentY, currentX, currentY - 1, out nextX, out nextY))
            {
                return (nextX, nextY);
            }

            return (currentX, currentY);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }

        private bool TryStepToward(int currentX, int currentY, int candidateX, int candidateY, out int nextX, out int nextY)
        {
            nextX = currentX;
            nextY = currentY;

            if (candidateX == currentX && candidateY == currentY)
            {
                return false;
            }

            var tile = _gridSystem.GetTile(candidateX, candidateY);
            if (tile == null || !tile.IsRoad)
            {
                return false;
            }

            nextX = candidateX;
            nextY = candidateY;
            return true;
        }
    }
}
