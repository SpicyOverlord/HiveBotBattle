using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    /// <summary>
    /// Represents a move made by a fighter bot.
    /// </summary>
    public readonly struct MotherShipMove
    {
        /// <summary>
        /// The type of the MotherShip move.
        /// </summary>
        public readonly MotherShipMoveType Type;

        /// <summary>
        /// Represents a move for the mother ship.
        /// </summary>
        /// <param name="motherShipMoveType">The type of move for the mother ship.</param>
        public MotherShipMove(MotherShipMoveType motherShipMoveType)
        {
            Type = motherShipMoveType;
        }
    }
}