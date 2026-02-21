using System.Linq;
using CityBuilder.Simulation;
using NUnit.Framework;

namespace CityBuilder.Tests.EditMode
{
    public class ServiceAndCitizenTests
    {
        [Test]
        public void Tick_ComputesSafetyAndHealthIndicators()
        {
            var grid = new GridSystem(12, 12);
            for (var x = 0; x < 12; x++)
            {
                grid.PlaceRoad(x, 1);
                grid.PlaceRoad(x, 2);
            }

            grid.PlaceBuilding(2, 2, BuildingType.Residential);
            grid.PlaceBuilding(3, 2, BuildingType.Residential);
            grid.PlaceBuilding(4, 2, BuildingType.Industrial);
            grid.PlaceBuilding(5, 2, BuildingType.Commercial);
            grid.PlaceBuilding(6, 2, BuildingType.PoliceStation);
            grid.PlaceBuilding(7, 2, BuildingType.FireStation);
            grid.PlaceBuilding(8, 2, BuildingType.Hospital);

            var simulation = new CitySimulation(grid, 1);
            simulation.Tick();

            Assert.That(simulation.CrimeIndex, Is.InRange(0f, 100f));
            Assert.That(simulation.FireRiskIndex, Is.InRange(0f, 100f));
            Assert.That(simulation.HealthIndex, Is.InRange(0f, 100f));
        }

        [Test]
        public void Citizens_MoveOnRoadTilesOnly()
        {
            var grid = new GridSystem(15, 15);
            for (var x = 0; x < 15; x++)
            {
                grid.PlaceRoad(x, 1);
                grid.PlaceRoad(x, 5);
            }
            for (var y = 0; y < 15; y++)
            {
                grid.PlaceRoad(1, y);
                grid.PlaceRoad(5, y);
            }

            grid.PlaceBuilding(2, 2, BuildingType.Residential);
            grid.PlaceBuilding(3, 2, BuildingType.Residential);
            grid.PlaceBuilding(2, 4, BuildingType.Industrial);
            grid.PlaceBuilding(4, 4, BuildingType.Commercial);

            var simulation = new CitySimulation(grid, 1);
            for (var i = 0; i < 8; i++)
            {
                simulation.Tick();
            }

            Assert.That(simulation.Citizens.Count, Is.GreaterThan(0));
            Assert.That(simulation.Citizens.All(c => grid.GetTile(c.RoadX, c.RoadY).IsRoad), Is.True);
        }
    }
}
