namespace CityBuilder.Simulation
{
    /// <summary>
    /// Lightweight citizen model moving on road hexes.
    /// </summary>
    public sealed class Citizen
    {
        public HexCoord RoadCoord { get; private set; }
        public HexCoord HomeCoord { get; }
        public HexCoord WorkCoord { get; }
        public HexCoord ShopCoord { get; }
        public HexCoord TargetRoadCoord { get; private set; }

        private int _phase;

        public Citizen(HexCoord homeCoord, HexCoord workCoord, HexCoord shopCoord, HexCoord roadCoord)
        {
            HomeCoord = homeCoord;
            WorkCoord = workCoord;
            ShopCoord = shopCoord;
            RoadCoord = roadCoord;
            TargetRoadCoord = roadCoord;
        }

        public void SetTargetRoad(HexCoord coord)
        {
            TargetRoadCoord = coord;
        }

        public void AdvancePhase()
        {
            _phase = (_phase + 1) % 3;
        }

        public int CurrentPhase => _phase; // 0 work, 1 shop, 2 home

        public bool IsAtTarget()
        {
            return RoadCoord.Equals(TargetRoadCoord);
        }

        public void Step(HexCoord next)
        {
            RoadCoord = next;
        }
    }
}
