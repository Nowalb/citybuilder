using CityBuilder.Simulation;
using NUnit.Framework;

namespace CityBuilder.Tests.EditMode
{
    public class BuildingTests
    {
        [Test]
        public void Constructor_SetsLevelOneAndInitialStats()
        {
            var building = new Building(BuildingType.Residential);

            Assert.That(building.Level, Is.EqualTo(1));
            Assert.That(building.Residents, Is.EqualTo(8));
            Assert.That(building.Jobs, Is.EqualTo(0));
            Assert.That(building.UpkeepCost, Is.EqualTo(2));
        }

        [Test]
        public void Upgrade_IncreasesLevelAndRecalculatesStats()
        {
            var building = new Building(BuildingType.Industrial);

            building.Upgrade();

            Assert.That(building.Level, Is.EqualTo(2));
            Assert.That(building.Residents, Is.EqualTo(0));
            Assert.That(building.Jobs, Is.EqualTo(20));
            Assert.That(building.UpkeepCost, Is.EqualTo(10));
        }
    }
}
