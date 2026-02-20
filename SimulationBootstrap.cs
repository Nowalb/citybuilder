using CityBuilder.Simulation;
using UnityEngine;

/// <summary>
/// Unity entry point only. Wires frame time to pure simulation ticks.
/// </summary>
public sealed class SimulationBootstrap : MonoBehaviour
{
    [SerializeField] private float tickIntervalSeconds = 1f;

    private GridSystem _gridSystem;
    private CitySimulation _simulation;
    private float _elapsed;

    private void Start()
    {
        _gridSystem = new GridSystem(50, 50);
        _simulation = new CitySimulation(_gridSystem);

        SeedTestCity();
        RunTick();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        while (_elapsed >= tickIntervalSeconds)
        {
            _elapsed -= tickIntervalSeconds;
            RunTick();
        }
    }

    private void RunTick()
    {
        _simulation.Tick();

        Debug.Log(
            $"Tick {_simulation.TickCount} | Residents: {_simulation.TotalResidents} | Jobs: {_simulation.TotalJobs} | " +
            $"Unemployed: {_simulation.Unemployed} | Income: {_simulation.Income} | Upkeep: {_simulation.Upkeep} | " +
            $"Balance: {_simulation.Balance}");
    }

    private void SeedTestCity()
    {
        _gridSystem.PlaceBuilding(10, 10, BuildingType.Residential);
        _gridSystem.PlaceBuilding(10, 11, BuildingType.Residential);
        _gridSystem.PlaceBuilding(12, 10, BuildingType.Commercial);
        _gridSystem.PlaceBuilding(13, 10, BuildingType.Industrial);
        _gridSystem.PlaceBuilding(14, 10, BuildingType.Service);
    }
}
