using CityCore;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests world/cell coordinate roundtrip operations.
/// </summary>
public class GridRoundtripTests
{
    [Test]
    public void WorldToCell_CellToWorld_RoundtripMatchesCell()
    {
        GameObject terrainGo = new GameObject("terrain_test");
        TerrainSystem terrain = terrainGo.AddComponent<TerrainSystem>();
        terrain.Generate(42, 8, 8, 0.1f);

        GameObject gridGo = new GameObject("grid_test");
        GridSystem grid = gridGo.AddComponent<GridSystem>();
        grid.Initialize(8, 8, 2f, terrain);

        Vector3 world = grid.CellToWorld(3, 4);
        Vector2Int cell = grid.WorldToCell(world);

        Assert.AreEqual(new Vector2Int(3, 4), cell);
        Object.DestroyImmediate(gridGo);
        Object.DestroyImmediate(terrainGo);
    }
}
