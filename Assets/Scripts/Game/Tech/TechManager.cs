using System.Collections.Generic;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Tech tree unlock manager using ScriptableObject definitions and prerequisite checks.
    /// </summary>
    public class TechManager : MonoBehaviour
    {
        [SerializeField] private TechTreeData techTreeData;

        private readonly HashSet<string> unlockedTechs = new HashSet<string>();

        /// <summary>
        /// Attempts to unlock a technology by id.
        /// </summary>
        public bool UnlockTech(string id)
        {
            TechDefinition definition = techTreeData.technologies.Find(t => t.id == id);
            if (definition == null || unlockedTechs.Contains(id)) return false;

            foreach (string prerequisite in definition.prerequisites)
            {
                if (!unlockedTechs.Contains(prerequisite)) return false;
            }

            unlockedTechs.Add(id);
            return true;
        }

        /// <summary>
        /// Returns true if technology has been unlocked.
        /// </summary>
        public bool IsUnlocked(string id)
        {
            return unlockedTechs.Contains(id);
        }

        /// <summary>
        /// No-op placeholder for future time-based research systems.
        /// </summary>
        public void MonthlyTick()
        {
        }

        /// <summary>
        /// Exposes unlocked tech IDs for save serialization.
        /// </summary>
        public List<string> GetUnlockedTechs()
        {
            return new List<string>(unlockedTechs);
        }

        /// <summary>
        /// Restores unlock state from save data.
        /// </summary>
        public void Restore(List<string> techIds)
        {
            unlockedTechs.Clear();
            foreach (string id in techIds)
            {
                unlockedTechs.Add(id);
            }
        }
    }
}
