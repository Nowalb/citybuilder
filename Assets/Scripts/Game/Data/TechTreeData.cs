using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// ScriptableObject containing full list of unlockable technologies.
    /// </summary>
    [CreateAssetMenu(fileName = "TechTreeData", menuName = "CityCore/Tech Tree Data")]
    public class TechTreeData : ScriptableObject
    {
        public List<TechDefinition> technologies = new List<TechDefinition>();
    }

    /// <summary>
    /// A single technology definition in the tree.
    /// </summary>
    [Serializable]
    public class TechDefinition
    {
        public string id;
        public string name;
        public int cost;
        public List<string> prerequisites = new List<string>();
    }
}
