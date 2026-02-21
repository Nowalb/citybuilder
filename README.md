# City Builder MVP (Unity-ready)

## Folder structure
- `Assets/Scripts/Simulation` → pure C# simulation logic (no MonoBehaviour)
- `Assets/Scripts/Unity` → Unity rendering/input/controllers
- `Assets/Tests/EditMode` → NUnit EditMode tests

## Hex architecture (axial)
- Grid uses `HexCoord(Q, R)` axial coordinates.
- Grid storage is `Dictionary<HexCoord, Tile>`.
- Neighbor system supports 6 directions.
- Foundations included:
  - `Distance(HexCoord a, HexCoord b)`
  - `GetTilesInRange(center, radius)`
  - chunk bucket id via `HexGridMath.GetChunkId(...)`

## Terrain generation
- Map starts as terrain-only (no initial roads/buildings).
- Terrain is generated deterministically from seed in simulation layer.
- Terrain types: `Grass`, `Forest`, `Hill`, `Water`.
- Water tiles block roads and buildings.

## Auto city growth
- Simulation starts from empty map and grows automatically:
  - first road appears near map center,
  - roads expand from existing roads,
  - buildings spawn near roads.
- Building placement still respects road adjacency rule.

## Math used (pointy-top hex)
- `HexToWorldPosition`:
  - `x = size * sqrt(3) * (q + r/2)`
  - `z = size * 3/2 * r`
- `WorldToHex`:
  - inverse axial projection + cube/axial rounding

## How to run
1. Create/open Unity 6 URP project.
2. Copy `Assets` folder from this repo into your Unity project.
3. Create empty GameObject named `Simulation`.
4. Add `SimulationBootstrap` + `HexGridRenderer` to that object.
5. (Optional) add `PlacementController` for manual placement debug.
6. Add `CityCameraController` to Main Camera.
7. Press Play.

## Stability notes
- Simulation layer is Unity-free.
- Placement checks `EventSystem.current.IsPointerOverGameObject()`.
- If EventSystem is missing, `PlacementController` creates one.
- Hex renderer uses shared mesh/materials to limit allocations.
