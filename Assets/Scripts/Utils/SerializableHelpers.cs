using System;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Utility types and wrappers supporting Unity JSON serialization constraints.
    /// </summary>
    public static class SerializableHelpers
    {
        /// <summary>
        /// Converts Vector3Int to serializable payload.
        /// </summary>
        public static SerializableVector3Int ToSerializable(this Vector3Int value)
        {
            return new SerializableVector3Int { x = value.x, y = value.y, z = value.z };
        }

        /// <summary>
        /// Converts serializable payload back to Vector3Int.
        /// </summary>
        public static Vector3Int ToVector3Int(this SerializableVector3Int value)
        {
            return new Vector3Int(value.x, value.y, value.z);
        }

        /// <summary>
        /// Serializes object to pretty-printed JSON.
        /// </summary>
        public static string ToJson<T>(T data)
        {
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// Deserializes JSON to provided type.
        /// </summary>
        public static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }

    /// <summary>
    /// Serializable replacement for Vector3Int.
    /// </summary>
    [Serializable]
    public struct SerializableVector3Int
    {
        public int x;
        public int y;
        public int z;
    }
}
