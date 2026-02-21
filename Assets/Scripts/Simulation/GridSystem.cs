using System;
using System.Collections.Generic;

namespace CityBuilder.Simulation
{
    /// <summary>
    /// Pure simulation hex grid backed by Dictionary for scalable sparse/chunked growth.
    /// Supports rectangular or true hexagon-shaped maps.
    /// </summary>
    public sealed class GridSystem
    {
        public readonly struct BuildingPlacement
        {
            public BuildingPlacement(Building building, HexCoord coord)
            {
                Building = building;
                Coord = coord;
            }

            public Building Building { get; }
            public HexCoord Coord { get; }
        }

        private readonly Dictionary<HexCoord, Tile> _tiles;
        private readonly List<Building> _buildings;
        private readonly List<BuildingPlacement> _placements;

        public int Width { get; }
        public int Height { get; }
        public int SideLength { get; }
        public bool IsHexShaped { get; }

        public IReadOnlyCollection<Tile> Tiles => _tiles.Values;
        public IReadOnlyList<Building> Buildings => _buildings;
        public IReadOnlyList<BuildingPlacement> Placements => _placements;

        public GridSystem(int width = 50, int height = 50, int terrainSeed = 1337, float waterThreshold = 0.12f)
        {
            Width = width;
            Height = height;
            SideLength = 0;
            IsHexShaped = false;

            _tiles = new Dictionary<HexCoord, Tile>(width * height);
            _buildings = new List<Building>();
            _placements = new List<BuildingPlacement>();

            for (var q = 0; q < Width; q++)
            {
                for (var r = 0; r < Height; r++)
                {
                    var coord = new HexCoord(q, r);
                    _tiles[coord] = new Tile(coord, ResolveTerrain(coord, terrainSeed, waterThreshold));
                }
            }
        }

        /// <summary>
        /// Creates map with dimensions derived from sideLength.
        /// When createHexShape is true, the map is a true hexagon with edge length sideLength.
        /// When false, the map is a rectangle of size (2*sideLength-1) x (2*sideLength-1).
        /// </summary>
        public GridSystem(int sideLength, int terrainSeed, float waterThreshold, bool createHexShape)
        {
            SideLength = Math.Max(1, sideLength);
            Width = SideLength * 2 - 1;
            Height = SideLength * 2 - 1;
            IsHexShaped = createHexShape;

            var capacity = createHexShape
                ? 3 * SideLength * (SideLength - 1) + 1
                : Width * Height;

            _tiles = new Dictionary<HexCoord, Tile>(capacity);
            _buildings = new List<Building>();
            _placements = new List<BuildingPlacement>();

            if (!createHexShape)
            {
                for (var q = 0; q < Width; q++)
                {
                    for (var r = 0; r < Height; r++)
                    {
                        var coord = new HexCoord(q, r);
                        _tiles[coord] = new Tile(coord, ResolveTerrain(coord, terrainSeed, waterThreshold));
                    }
                }

                return;
            }

            var radius = SideLength - 1;
            for (var q = -radius; q <= radius; q++)
            {
                var rMin = Math.Max(-radius, -q - radius);
                var rMax = Math.Min(radius, -q + radius);
                for (var r = rMin; r <= rMax; r++)
                {
                    var coord = new HexCoord(q, r);
                    _tiles[coord] = new Tile(coord, ResolveTerrain(coord, terrainSeed, waterThreshold));
                }
            }
        }

        public Tile GetTile(HexCoord coord) => _tiles.TryGetValue(coord, out var tile) ? tile : null;
        public bool Contains(HexCoord coord) => _tiles.ContainsKey(coord);

        public bool PlaceRoad(HexCoord coord)
        {
            var tile = GetTile(coord);
            return tile != null && tile.TrySetRoad();
        }

        public bool PlaceBuilding(HexCoord coord, BuildingType type)
        {
            if (!IsBuildableTile(coord) || type == BuildingType.Empty || !HasAdjacentRoad(coord))
            {
                return false;
            }

            var tile = _tiles[coord];
            var building = new Building(type);
            if (!tile.TryPlaceBuilding(building))
            {
                return false;
            }

            _buildings.Add(building);
            _placements.Add(new BuildingPlacement(building, coord));
            return true;
        }

        public List<Tile> GetNeighbors(HexCoord coord)
        {
            var result = new List<Tile>(6);
            foreach (var neighborCoord in coord.GetNeighbors())
            {
                if (_tiles.TryGetValue(neighborCoord, out var tile))
                {
                    result.Add(tile);
                }
            }

            return result;
        }

        public bool HasAdjacentRoad(HexCoord coord)
        {
            foreach (var neighbor in GetNeighbors(coord))
            {
                if (neighbor.IsRoad)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetAdjacentRoad(HexCoord coord, out HexCoord roadCoord)
        {
            foreach (var neighbor in GetNeighbors(coord))
            {
                if (neighbor.IsRoad)
                {
                    roadCoord = neighbor.Coord;
                    return true;
                }
            }

            roadCoord = default;
            return false;
        }

        public int Distance(HexCoord a, HexCoord b)
        {
            var dq = a.Q - b.Q;
            var dr = a.R - b.R;
            var ds = (-a.Q - a.R) - (-b.Q - b.R);
            return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
        }

        public List<Tile> GetTilesInRange(HexCoord center, int radius)
        {
            var result = new List<Tile>();
            foreach (var tile in _tiles.Values)
            {
                if (Distance(center, tile.Coord) <= radius)
                {
                    result.Add(tile);
                }
            }

            return result;
        }

        private bool IsBuildableTile(HexCoord coord)
        {
            var tile = GetTile(coord);
            return tile != null && tile.IsBuildableTerrain && !tile.IsRoad && !tile.HasBuilding;
        }

        private static TerrainType ResolveTerrain(HexCoord coord, int seed, float waterThreshold)
        {
            var n = Noise01(coord.Q, coord.R, seed);
            if (n < waterThreshold) return TerrainType.Water;
            if (n > 0.78f) return TerrainType.Hill;
            if (n > 0.52f) return TerrainType.Forest;
            return TerrainType.Grass;
        }

        private static float Noise01(int q, int r, int seed)
        {
            var value = Math.Sin((q * 12.9898 + r * 78.233 + seed * 0.1234) * 0.35) * 43758.5453;
            var frac = value - Math.Floor(value);
            return (float)(frac < 0 ? frac + 1.0 : frac);
        }
    }
}
