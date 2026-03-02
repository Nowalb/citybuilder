using CityCore;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests validating day-to-month conversion cadence.
/// </summary>
public class TickSchedulerTests
{
    [Test]
    public void MonthlyTick_TriggersEvery30Days()
    {
        GameObject go = new GameObject("scheduler_test");
        TickScheduler scheduler = go.AddComponent<TickScheduler>();
        int monthTicks = 0;
        scheduler.OnMonthTick += _ => monthTicks++;

        for (int i = 0; i < 60; i++)
        {
            scheduler.ForceSingleDayTick();
        }

        Assert.AreEqual(2, monthTicks);
        Object.DestroyImmediate(go);
    }
}
