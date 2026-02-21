using System;
using System.Collections.Generic;

namespace CityBuilder.Simulation
{
    /// <summary>
    /// Axial hex coordinate (q, r) for pointy-top hex grids.
    /// </summary>
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        private static readonly HexCoord[] NeighborOffsets =
        {
            new HexCoord(1, 0),
            new HexCoord(1, -1),
            new HexCoord(0, -1),
            new HexCoord(-1, 0),
            new HexCoord(-1, 1),
            new HexCoord(0, 1)
        };

        public int Q { get; }
        public int R { get; }

        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        public IEnumerable<HexCoord> GetNeighbors()
        {
            for (var i = 0; i < NeighborOffsets.Length; i++)
            {
                var d = NeighborOffsets[i];
                yield return new HexCoord(Q + d.Q, R + d.R);
            }
        }

        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;
        public override bool Equals(object obj) => obj is HexCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Q, R);

        public static HexCoord operator +(HexCoord a, HexCoord b) => new(a.Q + b.Q, a.R + b.R);
    }
}
