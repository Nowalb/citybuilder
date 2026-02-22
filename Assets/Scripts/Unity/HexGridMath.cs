using System;
using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Pointy-top hex math helpers for world/axial conversion.
    /// </summary>
    public static class HexGridMath
    {
        public static Vector3 HexToWorldPosition(HexCoord coord, float size)
        {
            var x = size * Mathf.Sqrt(3f) * (coord.Q + coord.R * 0.5f);
            var z = size * 1.5f * coord.R;
            return new Vector3(x, 0f, z);
        }

        public static HexCoord WorldToHex(Vector3 world, float size)
        {
            var q = (Mathf.Sqrt(3f) / 3f * world.x - 1f / 3f * world.z) / size;
            var r = (2f / 3f * world.z) / size;
            return AxialRound(q, r);
        }

        public static HexCoord AxialRound(float q, float r)
        {
            var x = q;
            var z = r;
            var y = -x - z;

            var rx = Mathf.RoundToInt(x);
            var ry = Mathf.RoundToInt(y);
            var rz = Mathf.RoundToInt(z);

            var xDiff = Mathf.Abs(rx - x);
            var yDiff = Mathf.Abs(ry - y);
            var zDiff = Mathf.Abs(rz - z);

            if (xDiff > yDiff && xDiff > zDiff)
            {
                rx = -ry - rz;
            }
            else if (yDiff > zDiff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new HexCoord(rx, rz);
        }

        public static int GetChunkId(HexCoord coord, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            }

            var cq = Mathf.FloorToInt((float)coord.Q / chunkSize);
            var cr = Mathf.FloorToInt((float)coord.R / chunkSize);
            return HashCode.Combine(cq, cr);
        }
    }
}
