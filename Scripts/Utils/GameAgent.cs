using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    /// <summary>
    /// Represents a game agent in the HiveBotBattle game.
    /// </summary>
    public abstract class GameAgent
    {
        /// <summary>
        /// The ID of the player that owns the game agent.
        /// </summary>
        public readonly int PlayerID;

        /// <summary>
        /// The position of the game agent.
        /// </summary>
        public Pos Pos { get; protected set; }

        /// <summary>
        /// The type of the game agent.
        /// </summary>
        public readonly BotType Type;

        /// <summary>
        /// Indicates whether the game agent is destroyed.
        /// </summary>
        public bool IsDestroyed { get; protected set; }

        /// <summary>
        /// The health of the game agent.
        /// </summary>
        public int Health { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameAgent"/> class.
        /// </summary>
        /// <param name="playerID">The ID of the player that owns the game agent.</param>
        /// <param name="botType">The type of the game agent.</param>
        /// <param name="startPos">The starting position of the game agent.</param>
        /// <param name="startHealth">The starting health of the game agent.</param>
        protected GameAgent(int playerID, BotType botType, Pos startPos, int startHealth)
        {
            PlayerID = playerID;
            Type = botType;
            Pos = startPos;
            Health = startHealth;
            IsDestroyed = false;
        }

        /// <summary>
        /// Moves the game agent to the specified position.
        /// </summary>
        /// <param name="pos">The position to move to.</param>
        public void MoveTo(Pos pos) => Pos = pos.Clone();

        /// <summary>
        /// Damages the game agent by the specified amount.
        /// </summary>
        /// <param name="damage">The amount of damage to inflict.</param>
        public void Damage(int damage)
        {
            if (IsDestroyed) return;

            Health -= damage;
            if (Health <= 0) IsDestroyed = true;
        }
    }
}