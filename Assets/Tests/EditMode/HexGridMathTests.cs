using CityBuilder.Simulation;
using CityBuilder.Unity;
using NUnit.Framework;

namespace CityBuilder.Tests.EditMode
{
    public class HexGridMathTests
    {
        [Test]
        public void WorldToHex_RoundTrip_PicksOriginalHex()
        {
            var coord = new HexCoord(7, 11);
            var world = HexGridMath.HexToWorldPosition(coord, 1f);
            var snapped = HexGridMath.WorldToHex(world, 1f);

            Assert.That(snapped, Is.EqualTo(coord));
        }
    }
}
