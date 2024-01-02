using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    /// <summary>
    /// Represents a move made by a fighter bot.
    /// </summary>
    public readonly struct FighterBotMove
    {
        /// <summary>
        /// The type of the fighter bot move.
        /// </summary>
        public readonly FighterMoveType Type;

        /// <summary>
        /// The target position of the fighter bot move.
        /// </summary>
        public readonly Pos TargetPos;

        /// <summary>
        /// Indicates whether the fighter bot move has a target position.
        /// </summary>
        public readonly bool HasTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="FighterBotMove"/> struct with a target position.
        /// </summary>
        /// <param name="fighterMoveType">The type of the fighter bot move.</param>
        /// <param name="targetPos">The target position of the fighter bot move.</param>
        public FighterBotMove(FighterMoveType fighterMoveType, Pos targetPos)
        {
            Type = fighterMoveType;
            TargetPos = targetPos;
            HasTarget = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FighterBotMove"/> struct with a target position specified by coordinates.
        /// </summary>
        /// <param name="fighterMoveType">The type of the fighter bot move.</param>
        /// <param name="x">The x-coordinate of the target position.</param>
        /// <param name="y">The y-coordinate of the target position.</param>
        public FighterBotMove(FighterMoveType fighterMoveType, int x, int y)
        {
            Type = fighterMoveType;
            TargetPos = new Pos(x, y);
            HasTarget = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FighterBotMove"/> struct without a target position.
        /// </summary>
        /// <param name="fighterMoveType">The type of the fighter bot move.</param>
        public FighterBotMove(FighterMoveType fighterMoveType)
        {
            Type = fighterMoveType;
            TargetPos = null;
            HasTarget = false;
        }
    }
}