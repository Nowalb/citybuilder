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
            var grid = new GridSystem(20, 20);

            for (var q = 0; q < 20; q++)
            {
                grid.PlaceRoad(new HexCoord(q, 5));
            }

            grid.PlaceBuilding(new HexCoord(6, 6), BuildingType.Residential);
            grid.PlaceBuilding(new HexCoord(7, 6), BuildingType.Residential);
            grid.PlaceBuilding(new HexCoord(8, 6), BuildingType.Industrial);
            grid.PlaceBuilding(new HexCoord(9, 6), BuildingType.Commercial);
            grid.PlaceBuilding(new HexCoord(10, 6), BuildingType.PoliceStation);
            grid.PlaceBuilding(new HexCoord(11, 6), BuildingType.FireStation);
            grid.PlaceBuilding(new HexCoord(12, 6), BuildingType.Hospital);

            var simulation = new CitySimulation(grid, 1);
            simulation.Tick();

            Assert.That(simulation.CrimeIndex, Is.InRange(0f, 100f));
            Assert.That(simulation.FireRiskIndex, Is.InRange(0f, 100f));
            Assert.That(simulation.HealthIndex, Is.InRange(0f, 100f));
        }

        [Test]
        public void Citizens_MoveOnRoadTilesOnly()
        {
            var grid = new GridSystem(20, 20);
            for (var q = 0; q < 20; q++)
            {
                grid.PlaceRoad(new HexCoord(q, 5));
                grid.PlaceRoad(new HexCoord(q, 6));
            }

            grid.PlaceBuilding(new HexCoord(6, 6), BuildingType.Residential);
            grid.PlaceBuilding(new HexCoord(7, 6), BuildingType.Residential);
            grid.PlaceBuilding(new HexCoord(8, 6), BuildingType.Industrial);
            grid.PlaceBuilding(new HexCoord(9, 6), BuildingType.Commercial);

            var simulation = new CitySimulation(grid, 1);
            for (var i = 0; i < 8; i++)
            {
                simulation.Tick();
            }

            Assert.That(simulation.Citizens.Count, Is.GreaterThan(0));
            Assert.That(simulation.Citizens.All(c => grid.GetTile(c.RoadCoord).IsRoad), Is.True);
        }
    }
}
