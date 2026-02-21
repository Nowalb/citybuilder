namespace CityBuilder.Simulation
{
    /// <summary>
    /// Pure simulation entity for one building.
    /// </summary>
    public sealed class Building
    {
        public BuildingType BuildingType { get; }
        public int Level { get; private set; }
        public int Residents { get; private set; }
        public int Jobs { get; private set; }
        public int UpkeepCost { get; private set; }

        public Building(BuildingType buildingType)
        {
            BuildingType = buildingType;
            Level = 1;
            UpdateStats();
        }

        public void Upgrade()
        {
            Level++;
            UpdateStats();
        }

        /// <summary>
        /// Recalculates simulation stats based on type + level.
        /// </summary>
        public void UpdateStats()
        {
            switch (BuildingType)
            {
                case BuildingType.Residential:
                    Residents = 8 * Level;
                    Jobs = 0;
                    UpkeepCost = 2 * Level;
                    break;
                case BuildingType.Commercial:
                    Residents = 0;
                    Jobs = 6 * Level;
                    UpkeepCost = 3 * Level;
                    break;
                case BuildingType.Industrial:
                    Residents = 0;
                    Jobs = 10 * Level;
                    UpkeepCost = 5 * Level;
                    break;
                case BuildingType.Service:
                    Residents = 0;
                    Jobs = 4 * Level;
                    UpkeepCost = 4 * Level;
                    break;
                default:
                    Residents = 0;
                    Jobs = 0;
                    UpkeepCost = 0;
                    break;
            }
        }
    }
}
