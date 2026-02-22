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
                    case BuildingType.PoliceStation: policeCount++; break;
                    case BuildingType.FireStation: fireStationCount++; break;
                    case BuildingType.Hospital: hospitalCount++; break;
                    case BuildingType.Industrial: industrialCount++; break;
                    case BuildingType.Commercial: commercialCount++; break;
                }
            }

            Unemployed = Math.Max(0, TotalResidents - TotalJobs);
            Income = TotalResidents * 2;
            Balance = Income - Upkeep;

            CrimeIndex = Clamp01((TotalResidents / 350f) - (policeCount * 0.20f)) * 100f;
            FireRiskIndex = Clamp01((industrialCount * 0.06f) + (commercialCount * 0.03f) + (TotalResidents / 1000f) - (fireStationCount * 0.20f)) * 100f;
            HealthIndex = Clamp01(0.45f + (hospitalCount * 0.18f) - (CrimeIndex / 200f) - (FireRiskIndex / 250f)) * 100f;
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
                var home = residences[_random.Next(residences.Count)].Coord;
                var work = workplaces[_random.Next(workplaces.Count)].Coord;
                var shop = shops[_random.Next(shops.Count)].Coord;

                if (!_gridSystem.TryGetAdjacentRoad(home, out var road))
                {
                    break;
                }

                var citizen = new Citizen(home, work, shop, road);
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

                citizen.Step(CalculateNextRoadStep(citizen.RoadCoord, citizen.TargetRoadCoord));
            }
        }

        private void SetCitizenTarget(Citizen citizen)
        {
            var destination = citizen.CurrentPhase switch
            {
                0 => citizen.WorkCoord,
                1 => citizen.ShopCoord,
                _ => citizen.HomeCoord
            };

            if (_gridSystem.TryGetAdjacentRoad(destination, out var road))
            {
                citizen.SetTargetRoad(road);
            }
        }

        private HexCoord CalculateNextRoadStep(HexCoord current, HexCoord target)
        {
            var neighbors = _gridSystem.GetNeighbors(current);
            HexCoord best = current;
            var bestDist = int.MaxValue;

            foreach (var neighbor in neighbors)
            {
                if (!neighbor.IsRoad)
                {
                    continue;
                }

                var d = _gridSystem.Distance(neighbor.Coord, target);
                if (d < bestDist)
                {
                    best = neighbor.Coord;
                    bestDist = d;
                }
            }

            return best;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }
    }
}
