namespace CityBuilder.Simulation
{
    /// <summary>
    /// Represents one grid cell that can hold at most one building.
    /// </summary>
    public sealed class Tile
    {
        public int X { get; }
        public int Y { get; }

        public Building Building { get; private set; }

        public bool HasBuilding => Building != null;

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool TryPlaceBuilding(Building building)
        {
            if (building == null || HasBuilding)
            {
                return false;
            }

            Building = building;
            return true;
        }
    }
}
