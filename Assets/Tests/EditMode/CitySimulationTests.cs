using CityBuilder.Simulation;
using NUnit.Framework;

namespace CityBuilder.Tests.EditMode
{
    public class CitySimulationTests
    {
        [Test]
        public void Tick_CalculatesPopulationJobsAndEconomy()
        {
            var grid = new GridSystem(10, 10, terrainSeed: 1, waterThreshold: 0f);
            grid.PlaceRoad(new HexCoord(2, 1));
            grid.PlaceRoad(new HexCoord(3, 2));
            grid.PlaceBuilding(new HexCoord(2, 2), BuildingType.Residential);
            grid.PlaceBuilding(new HexCoord(3, 3), BuildingType.Commercial);

            var simulation = new CitySimulation(grid, randomSeed: 1);
            simulation.Tick();

            Assert.That(simulation.TotalResidents, Is.EqualTo(8));
            Assert.That(simulation.TotalJobs, Is.EqualTo(6));
            Assert.That(simulation.Unemployed, Is.EqualTo(2));
            Assert.That(simulation.Income, Is.EqualTo(16));
            Assert.That(simulation.Upkeep, Is.EqualTo(5));
            Assert.That(simulation.Balance, Is.EqualTo(11));
            Assert.That(simulation.TickCount, Is.EqualTo(1));
        }
    }
}
