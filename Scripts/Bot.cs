using System;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;

namespace HiveBotBattle.Scripts
{
    /// <summary>
    /// Represents a bot in the HiveBotBattle game.
    /// </summary>
    public class Bot : GameAgent
    {
        /// <summary>
        /// The amount of damage the bot can inflict.
        /// </summary>
        public const int DamageAmount = 10;

        /// <summary>
        /// The range at which the bot can shoot.
        /// </summary>
        public const float ShootingRange = 2.85f;

        /// <summary>
        /// The constant value representing the change for the bot is hit.
        /// </summary>
        public const float HitChange = 0.75f;

        /// <summary>
        /// The unique identifier of the bot.
        /// </summary>
        public readonly int BotID;

        /// <summary>
        /// The maximum health of the bot.
        /// </summary>
        public const int MaxHealth = 100;

        /// <summary>
        /// The amount of health the bot can heal.
        /// </summary>
        public const int HealAmount = 5;

        /// <summary>
        /// The maximum number of minerals the bot can pick up.
        /// </summary>
        public const int MaxPickedUpMinerals = 3;

        /// <summary>
        /// The number of minerals the bot has currently picked up.
        /// </summary>
        public int PickedUpMinerals { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        /// <param name="playerID">The ID of the player controlling the bot.</param>
        /// <param name="botID">The unique identifier of the bot.</param>
        /// <param name="type">The type of the bot.</param>
        /// <param name="startPosition">The starting position of the bot.</param>
        public Bot(int playerID, int botID, AgentType type, Pos startPosition) : base(playerID, type, startPosition,
            MaxHealth)
        {
            BotID = botID;
            ShouldBeDestroyed = false;
        }

        /// <summary>
        /// Heals the bot by the specified amount.
        /// </summary>
        public void Heal()
        {
            Health += HealAmount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        /// <summary>
        /// Determines whether the bot can pick up more minerals.
        /// </summary>
        /// <returns><c>true</c> if the bot can pick up more minerals; otherwise, <c>false</c>.</returns>
        public bool CanPickUpMinerals() => PickedUpMinerals < MaxPickedUpMinerals;

        /// <summary>
        /// Adds a mineral to the bot's inventory.
        /// </summary>
        /// <returns><c>true</c> if the mineral was successfully added; otherwise, <c>false</c>.</returns>
        /// <exception cref="Exception">Thrown when the bot cannot pick up more minerals.</exception>
        public bool AddMineral()
        {
            if (!CanPickUpMinerals()) throw new Exception("Can't pick up more Minerals");
            PickedUpMinerals++;
            return true;
        }

        /// <summary>
        /// Removes all the minerals picked up by the bot.
        /// </summary>
        public void RemovePickedUpMinerals() => PickedUpMinerals = 0;
    }
}