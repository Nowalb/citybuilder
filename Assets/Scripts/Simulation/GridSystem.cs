using System.Collections.Generic;

namespace CityBuilder.Simulation
{
    /// <summary>
    /// Pure simulation grid container and placement API.
    /// </summary>
    public sealed class GridSystem
    {
        private readonly Tile[,] _tiles;
        private readonly List<Building> _buildings;

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Building> Buildings => _buildings;

        public GridSystem(int width = 50, int height = 50)
        {
            Width = width;
            Height = height;
            _tiles = new Tile[Width, Height];
            _buildings = new List<Building>();

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    _tiles[x, y] = new Tile(x, y);
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            return IsInBounds(x, y) ? _tiles[x, y] : null;
        }

        public bool PlaceBuilding(int x, int y, BuildingType type)
        {
            if (!IsInBounds(x, y) || type == BuildingType.Empty)
            {
                return false;
            }

            var tile = _tiles[x, y];
            var building = new Building(type);

            if (!tile.TryPlaceBuilding(building))
            {
                return false;
            }

            _buildings.Add(building);
            return true;
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
