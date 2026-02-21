using CityBuilder.Simulation;
using NUnit.Framework;

namespace CityBuilder.Tests.EditMode
{
    public class GridSystemTests
    {
        [Test]
        public void Constructor_CreatesDefault50x50Grid()
        {
            var grid = new GridSystem();

            Assert.That(grid.Width, Is.EqualTo(50));
            Assert.That(grid.Height, Is.EqualTo(50));
            Assert.That(grid.GetTile(0, 0), Is.Not.Null);
            Assert.That(grid.GetTile(49, 49), Is.Not.Null);
            Assert.That(grid.GetTile(50, 50), Is.Null);
        }

        [Test]
        public void PlaceBuilding_AllowsOneBuildingPerTile()
        {
            var grid = new GridSystem(5, 5);

            var firstPlacement = grid.PlaceBuilding(2, 2, BuildingType.Residential);
            var secondPlacement = grid.PlaceBuilding(2, 2, BuildingType.Industrial);

            Assert.That(firstPlacement, Is.True);
            Assert.That(secondPlacement, Is.False);
            Assert.That(grid.Buildings.Count, Is.EqualTo(1));
        }

        [Test]
        public void PlaceBuilding_RejectsOutOfBoundsAndEmptyType()
        {
            var grid = new GridSystem(5, 5);

            Assert.That(grid.PlaceBuilding(-1, 0, BuildingType.Residential), Is.False);
            Assert.That(grid.PlaceBuilding(6, 0, BuildingType.Residential), Is.False);
            Assert.That(grid.PlaceBuilding(0, 0, BuildingType.Empty), Is.False);
            Assert.That(grid.Buildings.Count, Is.EqualTo(0));
        }
    }
}
