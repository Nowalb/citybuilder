using System;
using System.Collections;
using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Coroutine-based game clock where one real-time second equals one in-game day.
    /// Emits day ticks and month ticks (every 30 days).
    /// </summary>
    public class TickScheduler : MonoBehaviour
    {
        public const int DaysPerMonth = 30;
        private const float MinSpeedMultiplier = 0.25f;
        private const float MaxSpeedMultiplier = 8f;

        /// <summary>
        /// Invoked each simulated day. Parameter is absolute day count.
        /// </summary>
        public event Action<int> OnDayTick;

        /// <summary>
        /// Invoked each simulated month (30 days). Parameter is absolute month count.
        /// </summary>
        public event Action<int> OnMonthTick;

        [SerializeField] private float secondsPerDay = 1f;

        private Coroutine loopRoutine;
        private bool isPaused;
        private int currentDay;
        private int currentMonth;
        private float speedMultiplier = 1f;

        /// <summary>
        /// Current day number starting from 1.
        /// </summary>
        public int CurrentDay => currentDay;

        /// <summary>
        /// Current month number starting from 1.
        /// </summary>
        public int CurrentMonth => currentMonth;

        /// <summary>
        /// Starts emitting ticks from day one.
        /// </summary>
        public void StartScheduler()
        {
            StopScheduler();
            currentDay = 0;
            currentMonth = 0;
            isPaused = false;
            loopRoutine = StartCoroutine(TickLoop());
        }

        /// <summary>
        /// Stops the ticking coroutine.
        /// </summary>
        public void StopScheduler()
        {
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }
        }

        /// <summary>
        /// Sets pause state for scheduler updates.
        /// </summary>
        public void SetPaused(bool paused)
        {
            isPaused = paused;
        }

        /// <summary>
        /// Adjusts game speed multiplier with safety clamping.
        /// </summary>
        public void FastForward(float multiplier)
        {
            speedMultiplier = Mathf.Clamp(multiplier, MinSpeedMultiplier, MaxSpeedMultiplier);
        }

        /// <summary>
        /// Executes one day immediately, useful for tests and admin controls.
        /// </summary>
        public void ForceSingleDayTick()
        {
            AdvanceOneDay();
        }

        private IEnumerator TickLoop()
        {
            while (true)
            {
                if (!isPaused)
                {
                    AdvanceOneDay();
                }

                float waitSeconds = secondsPerDay / speedMultiplier;
                yield return new WaitForSeconds(waitSeconds);
            }
        }

        private void AdvanceOneDay()
        {
            currentDay++;
            OnDayTick?.Invoke(currentDay);

            if (currentDay % DaysPerMonth == 0)
            {
                currentMonth++;
                OnMonthTick?.Invoke(currentMonth);
            }
        }
    }
}
