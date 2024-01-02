using HiveBotBattle.Scripts;

namespace Utils.Observations
{
    /// <summary>
    /// Represents an observation of the mother ship in the game.
    /// </summary>
    public class MotherShipObservation : Observation
    {
        private readonly MotherShip _motherShip;

        /// <summary>
        /// Initializes a new instance of the <see cref="MotherShipObservation"/> class.
        /// </summary>
        /// <param name="gameController">The game controller.</param>
        /// <param name="accMap">The accessibility map.</param>
        /// <param name="player">The player.</param>
        /// <param name="motherShip">The mother ship.</param>
        public MotherShipObservation(GameController gameController, PathFinder.AccessibilityMap accMap, Player player, MotherShip motherShip) :
            base(gameController, accMap, player, motherShip)
        {
            _motherShip = motherShip;
        }

        /// <summary>
        /// Gets the health of the mother ship as a percentage.
        /// </summary>
        /// <returns>The health of the mother ship as a percentage.</returns>
        public int GetHealthInPercent() => _motherShip.Health / (MotherShip.MaxHealth / 100);

        /// <summary>
        /// Gets the number of stored minerals by the player.
        /// </summary>
        /// <returns>The number of stored minerals by the player.</returns>
        public int GetStoredMinerals() => Player.StoredMinerals;

        /// <summary>
        /// Gets the build cost of a fighter unit for the player.
        /// </summary>
        /// <returns>The build cost of a fighter unit for the player.</returns>
        public int GetFighterBuildCost() => Player.GetCurrentFighterBuildCost();

        /// <summary>
        /// Gets the build cost of a miner unit for the player.
        /// </summary>
        /// <returns>The build cost of a miner unit for the player.</returns>
        public int GetMinerBuildCost() => Player.GetCurrentMinerBuildCost();
    }
}