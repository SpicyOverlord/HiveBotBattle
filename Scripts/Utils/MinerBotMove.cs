using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    /// <summary>
    /// Represents a move for a miner bot.
    /// </summary>
    public readonly struct MinerBotMove
    {
        /// <summary>
        /// Gets a value indicating whether the move has a target position.
        /// </summary>
        public readonly bool HasTarget;

        /// <summary>
        /// Gets the target position of the move.
        /// </summary>
        public readonly Pos TargetPos;

        /// <summary>
        /// Gets the type of the move.
        /// </summary>
        public readonly MinerMoveType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinerBotMove"/> struct with a target position.
        /// </summary>
        /// <param name="minerMoveType">The type of the move.</param>
        /// <param name="targetPos">The target position.</param>
        public MinerBotMove(MinerMoveType minerMoveType, Pos targetPos)
        {
            Type = minerMoveType;
            TargetPos = targetPos;
            HasTarget = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinerBotMove"/> struct with a target position specified by coordinates.
        /// </summary>
        /// <param name="minerMoveType">The type of the move.</param>
        /// <param name="x">The x-coordinate of the target position.</param>
        /// <param name="y">The y-coordinate of the target position.</param>
        public MinerBotMove(MinerMoveType minerMoveType, int x, int y)
        {
            Type = minerMoveType;
            TargetPos = new Pos(x, y);
            HasTarget = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinerBotMove"/> struct without a target position.
        /// </summary>
        /// <param name="minerMoveType">The type of the move.</param>
        public MinerBotMove(MinerMoveType minerMoveType)
        {
            Type = minerMoveType;
            TargetPos = null;
            HasTarget = false;
        }
    }
}