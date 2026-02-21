namespace CityBuilder.Simulation
{
    /// <summary>
    /// Pure simulation tile. Can be a road or hold a single building.
    /// </summary>
    public sealed class Tile
    {
        public HexCoord Coord { get; }
        public bool IsRoad { get; private set; }
        public Building Building { get; private set; }
        public bool HasBuilding => Building != null;

        public Tile(HexCoord coord)
        {
            Coord = coord;
        }

        public bool TrySetRoad()
        {
            if (HasBuilding)
            {
                return false;
            }

            IsRoad = true;
            return true;
        }

        public bool TryPlaceBuilding(Building building)
        {
            if (building == null || HasBuilding || IsRoad)
            {
                return false;
            }

            Building = building;
            return true;
        }
    }
}
