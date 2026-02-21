using CityBuilder.Simulation;
using NUnit.Framework;

namespace CityBuilder.Tests.EditMode
{
    public class GridSystemTests
    {
        [Test]
        public void Constructor_CreatesDefault50x50Grid()
        {
            var grid = new GridSystem(50, 50, terrainSeed: 1, waterThreshold: 0f);

            Assert.That(grid.Width, Is.EqualTo(50));
            Assert.That(grid.Height, Is.EqualTo(50));
            Assert.That(grid.GetTile(new HexCoord(0, 0)), Is.Not.Null);
            Assert.That(grid.GetTile(new HexCoord(49, 49)), Is.Not.Null);
            Assert.That(grid.GetTile(new HexCoord(50, 50)), Is.Null);
        }

        [Test]
        public void PlaceBuilding_RequiresAdjacentRoad()
        {
            var grid = new GridSystem(5, 5, terrainSeed: 1, waterThreshold: 0f);
            var coord = new HexCoord(2, 2);

            Assert.That(grid.PlaceBuilding(coord, BuildingType.Residential), Is.False);
            Assert.That(grid.PlaceRoad(new HexCoord(3, 2)), Is.True);
            Assert.That(grid.PlaceBuilding(coord, BuildingType.Residential), Is.True);
            Assert.That(grid.Buildings.Count, Is.EqualTo(1));
        }

        [Test]
        public void NeighborSystem_ReturnsUpToSixNeighbors()
        {
            var grid = new GridSystem(10, 10, terrainSeed: 1, waterThreshold: 0f);
            var neighbors = grid.GetNeighbors(new HexCoord(4, 4));
            Assert.That(neighbors.Count, Is.EqualTo(6));
        }

        [Test]
        public void HexDistance_Works()
        {
            var grid = new GridSystem(10, 10, terrainSeed: 1, waterThreshold: 0f);
            var dist = grid.Distance(new HexCoord(1, 1), new HexCoord(4, 3));
            Assert.That(dist, Is.EqualTo(5));
        }

        [Test]
        public void TerrainGeneration_CreatesWaterWhenThresholdIsHigh()
        {
            var grid = new GridSystem(10, 10, terrainSeed: 42, waterThreshold: 0.4f);
            var hasWater = false;
            foreach (var tile in grid.Tiles)
            {
                if (tile.TerrainType == TerrainType.Water)
                {
                    hasWater = true;
                    break;
                }
            }

            Assert.That(hasWater, Is.True);
        }
    }
}
