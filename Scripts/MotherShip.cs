using HiveBotBattle.Scripts.Utils.Types;
using Utils;

namespace HiveBotBattle.Scripts
{
    /// <summary>
    /// Represents a mother ship in the game.
    /// </summary>
    public class MotherShip : GameAgent
    {
        /// <summary>
        /// The maximum health of the mother ship.
        /// </summary>
        public const int MaxHealth = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="MotherShip"/> class.
        /// </summary>
        /// <param name="playerID">The ID of the player controlling the mother ship.</param>
        /// <param name="startPosition">The starting position of the mother ship.</param>
        public MotherShip(int playerID, Pos startPosition) :
            base(playerID, AgentType.MotherShip, startPosition, MaxHealth)
        { }
    }
}