using CityBuilder.Simulation;
using NUnit.Framework;

namespace CityBuilder.Tests.EditMode
{
    public class CitySimulationTests
    {
        [Test]
        public void Tick_CalculatesPopulationJobsAndEconomy()
        {
            var grid = new GridSystem(10, 10);
            grid.PlaceBuilding(1, 1, BuildingType.Residential); // +8 residents, +2 upkeep
            grid.PlaceBuilding(2, 2, BuildingType.Commercial);  // +6 jobs, +3 upkeep

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

        [Test]
        public void Tick_WhenNoUnemployment_UpgradesResidentialBuilding()
        {
            var grid = new GridSystem(10, 10);
            grid.PlaceBuilding(1, 1, BuildingType.Residential); // residents 8
            grid.PlaceBuilding(2, 2, BuildingType.Industrial);  // jobs 10 => unemployment 0

            var simulation = new CitySimulation(grid, randomSeed: 1);
            simulation.Tick();

            Assert.That(simulation.Unemployed, Is.EqualTo(6)); // after upgrade: residents 16, jobs 10
            Assert.That(simulation.TotalResidents, Is.EqualTo(16));
            Assert.That(simulation.TotalJobs, Is.EqualTo(10));
            Assert.That(simulation.Income, Is.EqualTo(32));
            Assert.That(simulation.Upkeep, Is.EqualTo(9));
            Assert.That(simulation.Balance, Is.EqualTo(23));
        }
    }
}
