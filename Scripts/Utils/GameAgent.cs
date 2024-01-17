using System;
using Godot;
using HiveBotBattle.Scripts;
using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    /// <summary>
    /// Represents a GameAgent in the HiveBotBattle game.
    /// </summary>
    public abstract class GameAgent
    {
        /// <summary>
        /// The ID of the player that owns the GameAgent.
        /// </summary>
        public readonly int PlayerID;

        /// <summary>
        /// The position of the GameAgent.
        /// </summary>
        public Pos Pos { get; protected set; }

        /// <summary>
        /// The type of the GameAgent.
        /// </summary>
        public readonly AgentType Type;

        /// <summary>
        /// Indicates whether the GameAgent is destroyed.
        /// </summary>
        public bool ShouldBeDestroyed { get; protected set; }

        /// <summary>
        /// The health of the GameAgent.
        /// </summary>
        public int Health { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameAgent"/> class.
        /// </summary>
        /// <param name="playerID">The ID of the player that owns the GameAgent.</param>
        /// <param name="botType">The type of the GameAgent.</param>
        /// <param name="startPos">The starting position of the GameAgent.</param>
        /// <param name="startHealth">The starting health of the GameAgent.</param>
        protected GameAgent(int playerID, AgentType botType, Pos startPos, int startHealth)
        {
            PlayerID = playerID;
            Type = botType;
            Pos = startPos;
            Health = startHealth;
            ShouldBeDestroyed = false;
        }

        /// <summary>
        /// Moves the GameAgent to the specified position.
        /// </summary>
        /// <param name="pos">The position to move to.</param>
        public void MoveTo(Pos pos) => Pos = pos.Clone();

        /// <summary>
        /// Damages the GameAgent by the specified amount.
        /// </summary>
        /// <param name="damage">The amount of damage to inflict.</param>
        public void Damage(int damage = Bot.DamageAmount)
        {
            if (ShouldBeDestroyed) return;

            // check if the bot is hit
            if (GD.Randi() % 100 < Bot.HitChange * 100)
                return;

            Health -= damage;
            if (Health <= 0) ShouldBeDestroyed = true;
        }
    }
}