# City Builder MVP (Unity-ready)

## Folder structure to paste into Unity
- `Assets/Scripts/Simulation` → pure C# simulation logic (no MonoBehaviour)
- `Assets/Scripts/Unity` → Unity rendering/input/controllers
- `Assets/Tests/EditMode` → NUnit EditMode tests

## Hex architecture (axial)
- Grid uses `HexCoord(Q, R)` axial coordinates.
- Grid storage is `Dictionary<HexCoord, Tile>`.
- Neighbor system supports 6 directions.
- Added foundations for:
  - `Distance(HexCoord a, HexCoord b)`
  - `GetTilesInRange(center, radius)`
  - chunk bucket id via `HexGridMath.GetChunkId(...)`

## Math used (pointy-top hex)
- `HexToWorldPosition`:
  - `x = size * sqrt(3) * (q + r/2)`
  - `z = size * 3/2 * r`
- `WorldToHex`:
  - inverse axial projection + cube rounding (axial rounding)

## How to run
1. Create/open Unity 6 URP project.
2. Copy `Assets` folder from this repo into your Unity project.
3. Create empty GameObject in scene named `Simulation`.
4. Add `CityBuilder.Unity.SimulationBootstrap` component.
5. Add `CityBuilder.Unity.PlacementController` component (same GO) and assign `bootstrap` reference.
6. Add `CityBuilder.Unity.CityCameraController` to Main Camera.
7. Press Play.

## Build UI (manual placement)
- Bottom build panel: Road, Residential, Industrial, Commercial, Police, Fire, Hospital.
- Left-click terrain to place selected type.
- Buildings require adjacent road and occupy 1 hex.
- Placement/raycast works on snapped nearest axial hex.

## Camera controls
- `WASD` / arrows = pan
- mouse near screen edge = edge pan
- mouse wheel = zoom in/out
- middle mouse drag = orbit
- `Q` / `E` = rotate left/right
- Works with both Input System package and legacy Input Manager
