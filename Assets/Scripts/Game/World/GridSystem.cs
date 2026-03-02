using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Logical square grid projected onto the generated terrain.
    /// </summary>
    public class GridSystem : MonoBehaviour
    {
        /// <summary>
        /// Single simulation cell storing occupancy and environmental constraints.
        /// </summary>
        public struct GridCell
        {
            public bool occupied;
            public string buildingId;
            public float slope;
            public bool isRoad;
        }

        private GridCell[,] cells;
        private float cellSize = 1f;
        private int width;
        private int height;

        /// <summary>
        /// Width in cells.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Height in cells.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Initializes grid and caches slope values from terrain.
        /// </summary>
        public void Initialize(int mapWidth, int mapHeight, float targetCellSize, TerrainSystem terrain)
        {
            width = mapWidth;
            height = mapHeight;
            cellSize = Mathf.Max(0.1f, targetCellSize);
            cells = new GridCell[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells[x, y].slope = terrain.GetSlopeAt(x, y);
                }
            }
        }

        /// <summary>
        /// Converts world position to cell coordinate.
        /// </summary>
        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            return new Vector2Int(Mathf.FloorToInt(worldPos.x / cellSize), Mathf.FloorToInt(worldPos.z / cellSize));
        }

        /// <summary>
        /// Converts cell coordinate to world center position.
        /// </summary>
        public Vector3 CellToWorld(int x, int y)
        {
            return new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
        }

        /// <summary>
        /// Checks whether a cell is available and slope-compliant.
        /// </summary>
        public bool IsCellFree(int x, int y, float maxSlope)
        {
            if (!InBounds(x, y)) return false;
            GridCell cell = cells[x, y];
            return !cell.occupied && !cell.isRoad && cell.slope <= maxSlope;
        }

        /// <summary>
        /// Marks cell as occupied by building id.
        /// </summary>
        public void OccupyCell(int x, int y, string buildingId)
        {
            if (!InBounds(x, y)) return;
            cells[x, y].occupied = true;
            cells[x, y].buildingId = buildingId;
        }

        /// <summary>
        /// Frees occupied building data from the selected cell.
        /// </summary>
        public void FreeCell(int x, int y)
        {
            if (!InBounds(x, y)) return;
            cells[x, y].occupied = false;
            cells[x, y].buildingId = string.Empty;
        }

        /// <summary>
        /// Sets road state for a cell.
        /// </summary>
        public void SetRoad(int x, int y, bool value)
        {
            if (!InBounds(x, y)) return;
            cells[x, y].isRoad = value;
        }

        /// <summary>
        /// Returns cell snapshot.
        /// </summary>
        public GridCell GetCell(int x, int y)
        {
            return InBounds(x, y) ? cells[x, y] : default;
        }

        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }
    }
}
