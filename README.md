# City Builder MVP (Unity-ready)

## Folder structure to paste into Unity
- `Assets/Scripts/Simulation` → pure C# simulation logic (no MonoBehaviour)
- `Assets/Scripts/Unity` → Unity adapter and visual generation
- `Assets/Tests/EditMode` → NUnit EditMode tests

## How to run
1. Create/open Unity 2021+ 3D project.
2. Copy `Assets` folder from this repo into your Unity project.
3. Create empty GameObject in scene named `Simulation`.
4. Add `CityBuilder.Unity.SimulationBootstrap` component.
5. Press Play.

## What you will see
- Full 50x50 map generated as quads.
- Road network generated automatically (gray tiles).
- City fills itself over time (configurable `buildingsPlacedPerTick`).
- Building placement is allowed only on tiles adjacent to roads.
- Safety-service buildings are included in growth:
  - Police Station
  - Fire Station
  - Hospital
- Building colors:
  - Residential / domy = green
  - Industrial / firmy = yellow
  - Commercial / komercyjne = blue
  - Police = dark blue
  - Fire Station = red
  - Hospital = white
- Indicators in logs every tick:
  - Crime index
  - Fire risk index
  - Health index
- Citizens visualized as small white dots moving only on roads between home/work/shop.
