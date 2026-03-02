using System.Collections.Generic;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Maintains logical road network connectivity and high-level congestion metrics.
    /// </summary>
    public class RoadManager : MonoBehaviour
    {
        private readonly HashSet<Vector2Int> roadCells = new HashSet<Vector2Int>();
        private GridSystem gridSystem;

        /// <summary>
        /// Number of road cells in network.
        /// </summary>
        public int RoadLength => roadCells.Count;

        /// <summary>
        /// Sets internal references and clears road data.
        /// </summary>
        public void Initialize(GridSystem grid)
        {
            gridSystem = grid;
            roadCells.Clear();
        }

        /// <summary>
        /// Draws logical road along a polyline sequence of grid cells.
        /// </summary>
        public void StrokeRoad(IReadOnlyList<Vector2Int> cells)
        {
            foreach (Vector2Int cell in cells)
            {
                roadCells.Add(cell);
                gridSystem.SetRoad(cell.x, cell.y, true);
            }
        }

        /// <summary>
        /// Returns true when any road exists in Manhattan radius around target cell.
        /// </summary>
        public bool HasRoadNearCell(int x, int y, int radius)
        {
            for (int iy = -radius; iy <= radius; iy++)
            {
                for (int ix = -radius; ix <= radius; ix++)
                {
                    if (roadCells.Contains(new Vector2Int(x + ix, y + iy)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates coarse congestion metric based on buildings served per road length.
        /// </summary>
        public float GetCongestionScore(int districtBuildingCount)
        {
            int denominator = Mathf.Max(1, roadCells.Count);
            return Mathf.Clamp01((float)districtBuildingCount / denominator);
        }
    }
}
