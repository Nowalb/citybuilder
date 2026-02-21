# City Builder MVP (Unity-ready)

## Folder structure to paste into Unity
- `Assets/Scripts/Simulation` → pure C# simulation logic (no MonoBehaviour)
- `Assets/Scripts/Unity` → Unity adapter and visual generation

## How to run
1. Create/open Unity 2021+ 3D project.
2. Copy `Assets` folder from this repo into your Unity project.
3. Create empty GameObject in scene named `Simulation`.
4. Add `CityBuilder.Unity.SimulationBootstrap` component.
5. Press Play.

What happens:
- 50x50 city grid is generated as tile GameObjects.
- Seed buildings are spawned and visualized as cubes.
- Simulation ticks every 1 second and logs economy/population values.
- Buildings can visually grow on upgrade (height scales with level).
