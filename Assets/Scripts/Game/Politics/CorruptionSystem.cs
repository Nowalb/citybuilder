using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Handles corruption level, bribe offers and policy decisions affecting systemic risk.
    /// </summary>
    public class CorruptionSystem : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float corruptionLevel = 5f;
        [SerializeField] private float monthlyDecay = 0.5f;

        public float CorruptionLevel => corruptionLevel;

        /// <summary>
        /// Proposes a bribe that grants cash now but increases corruption and scandal risk.
        /// </summary>
        public int OfferBribe(int amount, string effectId)
        {
            corruptionLevel = Mathf.Clamp(corruptionLevel + Mathf.Max(1, amount / 5000f), 0f, 100f);
            return amount;
        }

        /// <summary>
        /// Applies political policy choices optionally accepting associated bribes.
        /// </summary>
        public void ApplyPolicy(string policyId, bool acceptBribe = false)
        {
            if (acceptBribe)
            {
                corruptionLevel = Mathf.Clamp(corruptionLevel + 7f, 0f, 100f);
            }
            else
            {
                corruptionLevel = Mathf.Clamp(corruptionLevel - 2f, 0f, 100f);
            }
        }

        /// <summary>
        /// Probability boost for negative corruption events.
        /// </summary>
        public float GetNegativeEventMultiplier()
        {
            const float threshold = 55f;
            if (corruptionLevel <= threshold) return 1f;
            return 1f + (corruptionLevel - threshold) / 45f;
        }

        /// <summary>
        /// Applies baseline monthly anti-corruption drift.
        /// </summary>
        public void MonthlyTick()
        {
            corruptionLevel = Mathf.Clamp(corruptionLevel - monthlyDecay, 0f, 100f);
        }

        /// <summary>
        /// Restores corruption level from save payload.
        /// </summary>
        public void Restore(float value)
        {
            corruptionLevel = Mathf.Clamp(value, 0f, 100f);
        }
    }
}
