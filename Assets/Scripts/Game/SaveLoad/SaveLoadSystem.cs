using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// JSON persistence service storing and restoring complete simulation state in persistentDataPath.
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour
    {
        [Serializable]
        public class GameSaveData
        {
            public int terrainSeed;
            public List<BuildingManager.BuildingInstance> buildings = new List<BuildingManager.BuildingInstance>();
            public long budget;
            public long debt;
            public int popLow;
            public int popMid;
            public int popHigh;
            public float corruptionLevel;
            public List<string> unlockedTechs = new List<string>();
        }

        private const string DefaultFileName = "city_save.json";

        private GameManager gameManager;
        private BuildingManager buildingManager;
        private EconomySystem economySystem;
        private PopulationSystem populationSystem;
        private CorruptionSystem corruptionSystem;
        private TechManager techManager;
        private TerrainSystem terrainSystem;

        /// <summary>
        /// Initializes dependency references.
        /// </summary>
        public void Initialize(
            GameManager game,
            BuildingManager buildings,
            EconomySystem economy,
            PopulationSystem population,
            CorruptionSystem corruption,
            TechManager tech,
            TerrainSystem terrain)
        {
            gameManager = game;
            buildingManager = buildings;
            economySystem = economy;
            populationSystem = population;
            corruptionSystem = corruption;
            techManager = tech;
            terrainSystem = terrain;
        }

        /// <summary>
        /// Saves current state to JSON file.
        /// </summary>
        public void Save(string fileName = DefaultFileName)
        {
            GameSaveData data = new GameSaveData
            {
                terrainSeed = terrainSystem.CurrentSeed,
                buildings = new List<BuildingManager.BuildingInstance>(buildingManager.Instances),
                budget = economySystem.Budget,
                debt = economySystem.Debt,
                popLow = populationSystem.LowPopulation,
                popMid = populationSystem.MidPopulation,
                popHigh = populationSystem.HighPopulation,
                corruptionLevel = corruptionSystem.CorruptionLevel,
                unlockedTechs = techManager.GetUnlockedTechs()
            };

            string path = GetPath(fileName);
            File.WriteAllText(path, SerializableHelpers.ToJson(data));
            Debug.Log($"Game saved at: {path}");
        }

        /// <summary>
        /// Loads state from JSON file if it exists.
        /// </summary>
        public bool Load(string fileName = DefaultFileName)
        {
            string path = GetPath(fileName);
            if (!File.Exists(path)) return false;

            string json = File.ReadAllText(path);
            GameSaveData data = SerializableHelpers.FromJson<GameSaveData>(json);
            if (data == null) return false;

            gameManager.RegenerateWorld(data.terrainSeed);
            buildingManager.ClearAll();
            foreach (BuildingManager.BuildingInstance instance in data.buildings)
            {
                buildingManager.RestoreBuilding(instance);
            }

            economySystem.Restore(data.budget, data.debt);
            populationSystem.Restore(data.popLow, data.popMid, data.popHigh);
            corruptionSystem.Restore(data.corruptionLevel);
            techManager.Restore(data.unlockedTechs ?? new List<string>());
            return true;
        }

        /// <summary>
        /// Returns absolute save path for a file name.
        /// </summary>
        public string GetPath(string fileName = DefaultFileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}
