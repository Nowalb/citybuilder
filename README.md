# CityBuilder Skeleton (Unity / C#)

Projekt szkieletu city-builder w namespace `CityCore` dla Unity LTS (2021.3+ / 2022.3+).

## Uruchomienie
1. Otwórz folder projektu w Unity Hub.
2. Otwórz scenę `Assets/Scenes/Main.unity`.
3. Utwórz obiekt `GameManager` i dodaj komponenty:
   - `GameManager`, `TickScheduler`, `TerrainSystem`, `GridSystem`, `BuildingManager`,
   - `RoadManager`, `EconomySystem`, `PopulationSystem`, `CorruptionSystem`, `TechManager`, `EventSystem`, `SaveLoadSystem`, `UIManager`.
4. Podepnij referencje w inspectorze (zgodnie z polami `[SerializeField]`).
5. Uruchom Play.

## Seed / Heightmap
- Terrain generuje się przez `TerrainSystem.Generate(seed, width, height, scale)`.
- Seed możesz skopiować przyciskiem `Export Seed` (clipboard).

## Save / Load
- JSON zapisuje się do `Application.persistentDataPath/city_save.json`.
- UI zawiera przyciski Save i Load (wymagają przypięcia Buttonów).

## Wygenerowane pliki
- `Assets/Scripts/Game/Core/GameManager.cs`
- `Assets/Scripts/Game/Core/TickScheduler.cs`
- `Assets/Scripts/Game/World/TerrainSystem.cs`
- `Assets/Scripts/Game/World/GridSystem.cs`
- `Assets/Scripts/Game/Construction/PlacementValidator.cs`
- `Assets/Scripts/Game/Construction/BuildingManager.cs`
- `Assets/Scripts/Game/Data/BuildingData.cs`
- `Assets/Scripts/Game/Data/TechTreeData.cs`
- `Assets/Scripts/Game/Roads/RoadManager.cs`
- `Assets/Scripts/Game/Economy/EconomySystem.cs`
- `Assets/Scripts/Game/Population/PopulationSystem.cs`
- `Assets/Scripts/Game/Politics/CorruptionSystem.cs`
- `Assets/Scripts/Game/Events/EventSystem.cs`
- `Assets/Scripts/Game/Tech/TechManager.cs`
- `Assets/Scripts/Game/SaveLoad/SaveLoadSystem.cs`
- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/Utils/SerializableHelpers.cs`
- `Assets/Scenes/Main.unity`
- `Assets/ScriptableObjects/SampleBuildings.asset`
- `Assets/Resources/ScriptableObjects/*.asset`
- `Assets/Prefabs/*.prefab`
- `Assets/Tests/EditMode/TickSchedulerTests.cs`
- `Assets/Tests/EditMode/GridRoundtripTests.cs`
- `Assets/Tests/EditMode/SaveLoadSystemTests.cs`
- `Assets/ArtPlaceholders/readme.txt`
- `devchecklist.md`
- `trailer_shots.txt`
- `limits.txt`

## Roadmap
- Podmiana placeholder art na finalny low-poly pack.
- Rozbudowa BuildTool i zoning UI.
- Balans ekonomii i chain eventów.
