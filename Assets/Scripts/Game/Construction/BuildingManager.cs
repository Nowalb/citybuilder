using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Handles building placement, runtime instances, and aggregate queries used by simulation systems.
    /// </summary>
    public class BuildingManager : MonoBehaviour
    {
        /// <summary>
        /// Persistent building placement state.
        /// </summary>
        [Serializable]
        public class BuildingInstance
        {
            public string uid;
            public string buildingId;
            public int x;
            public int y;
            public float rotationY;
        }

        [SerializeField] private PlacementValidator placementValidator;

        private readonly List<BuildingInstance> instances = new List<BuildingInstance>();
        private readonly Dictionary<string, BuildingData> buildingDataById = new Dictionary<string, BuildingData>();
        private GridSystem gridSystem;
        private RoadManager roadManager;
        private EconomySystem economySystem;

        /// <summary>
        /// All active building instances.
        /// </summary>
        public IReadOnlyList<BuildingInstance> Instances => instances;

        /// <summary>
        /// Initializes dependencies and caches building catalog from Resources/ScriptableObjects.
        /// </summary>
        public void Initialize(GridSystem grid, RoadManager roads, EconomySystem economy)
        {
            gridSystem = grid;
            roadManager = roads;
            economySystem = economy;
            instances.Clear();
            buildingDataById.Clear();

            foreach (BuildingData data in Resources.LoadAll<BuildingData>("ScriptableObjects"))
            {
                if (!string.IsNullOrWhiteSpace(data.id))
                {
                    buildingDataById[data.id] = data;
                }
            }
        }

        /// <summary>
        /// Attempts to place a building at selected grid cell.
        /// </summary>
        public BuildingInstance PlaceBuilding(BuildingData data, int x, int y)
        {
            if (!placementValidator.CanPlace(data, x, y, gridSystem, roadManager))
            {
                return null;
            }

            if (!economySystem.Spend(data.cost))
            {
                return null;
            }

            gridSystem.OccupyCell(x, y, data.id);
            Vector3 worldPos = gridSystem.CellToWorld(x, y);
            if (data.prefab != null)
            {
                Instantiate(data.prefab, worldPos, Quaternion.identity, transform);
            }

            BuildingInstance instance = new BuildingInstance
            {
                uid = Guid.NewGuid().ToString("N"),
                buildingId = data.id,
                x = x,
                y = y,
                rotationY = 0f
            };

            instances.Add(instance);
            return instance;
        }

        /// <summary>
        /// Recreates building from save payload without charging player.
        /// </summary>
        public void RestoreBuilding(BuildingInstance instance)
        {
            if (!buildingDataById.TryGetValue(instance.buildingId, out BuildingData data)) return;
            gridSystem.OccupyCell(instance.x, instance.y, instance.buildingId);
            Vector3 worldPos = gridSystem.CellToWorld(instance.x, instance.y);
            if (data.prefab != null)
            {
                Instantiate(data.prefab, worldPos, Quaternion.Euler(0f, instance.rotationY, 0f), transform);
            }

            instances.Add(instance);
        }

        /// <summary>
        /// Returns monthly upkeep sum across all placed buildings.
        /// </summary>
        public long GetTotalUpkeep()
        {
            long total = 0;
            foreach (BuildingInstance instance in instances)
            {
                if (buildingDataById.TryGetValue(instance.buildingId, out BuildingData data))
                {
                    total += data.upkeepPerMonth;
                }
            }

            return total;
        }

        /// <summary>
        /// Returns available residential capacity from active buildings.
        /// </summary>
        public int GetResidentialCapacity()
        {
            int total = 0;
            foreach (BuildingInstance instance in instances)
            {
                if (buildingDataById.TryGetValue(instance.buildingId, out BuildingData data) &&
                    data.category == BuildingData.BuildingCategory.Residential)
                {
                    total += data.populationCapacity;
                }
            }

            return total;
        }

        /// <summary>
        /// Clears runtime instances for a complete load/reset operation.
        /// </summary>
        public void ClearAll()
        {
            instances.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
