using System.Collections.Generic;

namespace CityBuilder.Simulation
{
    /// <summary>
    /// Pure simulation grid container. Buildings require adjacency to roads.
    /// </summary>
    public sealed class GridSystem
    {
        public readonly struct BuildingPlacement
        {
            public BuildingPlacement(Building building, int x, int y)
            {
                Building = building;
                X = x;
                Y = y;
            }

            public Building Building { get; }
            public int X { get; }
            public int Y { get; }
        }

        private readonly Tile[,] _tiles;
        private readonly List<Building> _buildings;
        private readonly List<BuildingPlacement> _placements;

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Building> Buildings => _buildings;
        public IReadOnlyList<BuildingPlacement> Placements => _placements;

        public GridSystem(int width = 50, int height = 50)
        {
            Width = width;
            Height = height;
            _tiles = new Tile[Width, Height];
            _buildings = new List<Building>();
            _placements = new List<BuildingPlacement>();

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

        public bool PlaceRoad(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            return _tiles[x, y].TrySetRoad();
        }

        public bool PlaceBuilding(int x, int y, BuildingType type)
        {
            if (!IsBuildableTile(x, y) || type == BuildingType.Empty || !HasAdjacentRoad(x, y))
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
            _placements.Add(new BuildingPlacement(building, x, y));
            return true;
        }

        public bool HasAdjacentRoad(int x, int y)
        {
            return IsRoadAt(x + 1, y) || IsRoadAt(x - 1, y) || IsRoadAt(x, y + 1) || IsRoadAt(x, y - 1);
        }

        public bool TryGetAdjacentRoad(int x, int y, out int roadX, out int roadY)
        {
            if (IsRoadAt(x + 1, y))
            {
                roadX = x + 1;
                roadY = y;
                return true;
            }

            if (IsRoadAt(x - 1, y))
            {
                roadX = x - 1;
                roadY = y;
                return true;
            }

            if (IsRoadAt(x, y + 1))
            {
                roadX = x;
                roadY = y + 1;
                return true;
            }

            if (IsRoadAt(x, y - 1))
            {
                roadX = x;
                roadY = y - 1;
                return true;
            }

            roadX = -1;
            roadY = -1;
            return false;
        }

        private bool IsRoadAt(int x, int y)
        {
            return IsInBounds(x, y) && _tiles[x, y].IsRoad;
        }

        private bool IsBuildableTile(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            var tile = _tiles[x, y];
            return !tile.IsRoad && !tile.HasBuilding;
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
