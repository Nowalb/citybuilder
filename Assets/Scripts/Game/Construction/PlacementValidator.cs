using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Validates whether a selected grid cell can host a given building.
    /// </summary>
    public class PlacementValidator : MonoBehaviour
    {
        private const int DefaultRoadRadius = 2;

        /// <summary>
        /// Returns true when placement constraints are satisfied.
        /// </summary>
        public bool CanPlace(BuildingData data, int x, int y, GridSystem gridSystem, RoadManager roadManager)
        {
            if (data == null || gridSystem == null) return false;
            if (!gridSystem.IsCellFree(x, y, data.maxSlope)) return false;

            if (data.requiredRoadAccess)
            {
                return roadManager != null && roadManager.HasRoadNearCell(x, y, DefaultRoadRadius);
            }

            return true;
        }
    }
}
