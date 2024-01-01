using HiveBotBattle.Scripts;

namespace Utils.Observations
{
    public class MotherShipObservation : Observation
    {
        private readonly MotherShip _motherShip;

        public MotherShipObservation(GameController gameController, PathFinder.AccessibilityMap accMap, Player player, MotherShip motherShip) :
            base(gameController, accMap, player, motherShip)
        {
            _motherShip = motherShip;
        }

        public int GetHealthInPercent() => _motherShip.Health / (MotherShip.MaxHealth / 100);

        public int GetStoredMinerals() => Player.StoredMinerals;

        public int GetFighterBuildCost() => Player.GetCurrentFighterBuildCost();

        public int GetMinerBuildCost() => Player.GetCurrentMinerBuildCost();
    }
}