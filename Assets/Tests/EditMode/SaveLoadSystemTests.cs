using System.Collections.Generic;
using System.IO;
using CityCore;
using NUnit.Framework;

/// <summary>
/// Save/load payload roundtrip tests for building list consistency.
/// </summary>
public class SaveLoadSystemTests
{
    [Test]
    public void SaveLoadData_BuildingListRoundtrip_Consistent()
    {
        SaveLoadSystem.GameSaveData input = new SaveLoadSystem.GameSaveData
        {
            terrainSeed = 7,
            budget = 100,
            debt = 5,
            buildings = new List<BuildingManager.BuildingInstance>
            {
                new BuildingManager.BuildingInstance { uid = "a", buildingId = "residential_small", x = 1, y = 2, rotationY = 0f },
                new BuildingManager.BuildingInstance { uid = "b", buildingId = "park", x = 3, y = 5, rotationY = 90f }
            }
        };

        string json = SerializableHelpers.ToJson(input);
        SaveLoadSystem.GameSaveData output = SerializableHelpers.FromJson<SaveLoadSystem.GameSaveData>(json);

        Assert.AreEqual(input.buildings.Count, output.buildings.Count);
        Assert.AreEqual(input.buildings[0].buildingId, output.buildings[0].buildingId);
        Assert.AreEqual(input.buildings[1].x, output.buildings[1].x);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "citycore_saveload_test.json"), json);
    }
}
