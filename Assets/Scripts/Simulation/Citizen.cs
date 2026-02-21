namespace CityBuilder.Simulation
{
    /// <summary>
    /// Lightweight citizen model moving on road tiles.
    /// </summary>
    public sealed class Citizen
    {
        public int RoadX { get; private set; }
        public int RoadY { get; private set; }

        public int HomeX { get; }
        public int HomeY { get; }
        public int WorkX { get; }
        public int WorkY { get; }
        public int ShopX { get; }
        public int ShopY { get; }

        public int TargetRoadX { get; private set; }
        public int TargetRoadY { get; private set; }

        private int _phase;

        public Citizen(int homeX, int homeY, int workX, int workY, int shopX, int shopY, int roadX, int roadY)
        {
            HomeX = homeX;
            HomeY = homeY;
            WorkX = workX;
            WorkY = workY;
            ShopX = shopX;
            ShopY = shopY;
            RoadX = roadX;
            RoadY = roadY;
            TargetRoadX = roadX;
            TargetRoadY = roadY;
        }

        public void SetTargetRoad(int x, int y)
        {
            TargetRoadX = x;
            TargetRoadY = y;
        }

        public void AdvancePhase()
        {
            _phase = (_phase + 1) % 3;
        }

        public int CurrentPhase => _phase; // 0 work, 1 shop, 2 home

        public bool IsAtTarget()
        {
            return RoadX == TargetRoadX && RoadY == TargetRoadY;
        }

        public void Step(int nextX, int nextY)
        {
            RoadX = nextX;
            RoadY = nextY;
        }
    }
}
