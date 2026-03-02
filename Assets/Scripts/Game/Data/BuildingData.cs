using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Configurable data definition for a placeable building type.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingData", menuName = "CityCore/Building Data")]
    public class BuildingData : ScriptableObject
    {
        /// <summary>
        /// Gameplay building category.
        /// </summary>
        public enum BuildingCategory
        {
            Residential,
            Commercial,
            Industrial,
            Public
        }

        public string id;
        public string displayName;
        public int cost;
        public int upkeepPerMonth;
        public BuildingCategory category;
        public int populationCapacity;
        public int electricityUsage;
        public float maxSlope = 0.3f;
        public bool requiredRoadAccess;
        public GameObject prefab;
    }
}
