namespace CityBuilder.Simulation
{
    /// <summary>
    /// Pure simulation tile. Stores terrain and optional road/building.
    /// </summary>
    public sealed class Tile
    {
        public HexCoord Coord { get; }
        public TerrainType TerrainType { get; }
        public bool IsRoad { get; private set; }
        public Building Building { get; private set; }
        public bool HasBuilding => Building != null;
        public bool IsBuildableTerrain => TerrainType != TerrainType.Water;

        public Tile(HexCoord coord, TerrainType terrainType)
        {
            Coord = coord;
            TerrainType = terrainType;
        }

        public bool TrySetRoad()
        {
            if (HasBuilding || !IsBuildableTerrain)
            {
                return false;
            }

            IsRoad = true;
            return true;
        }

        public bool TryPlaceBuilding(Building building)
        {
            if (building == null || HasBuilding || IsRoad || !IsBuildableTerrain)
            {
                return false;
            }

            Building = building;
            return true;
        }
    }
}
