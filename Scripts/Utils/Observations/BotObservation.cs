using HiveBotBattle.Scripts;

namespace Utils.Observations
{
    /// <summary>
    /// Represents an observation of a bot in the game.
    /// </summary>
    public class BotObservation : Observation
    {
        private readonly Bot _bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotObservation"/> class.
        /// </summary>
        /// <param name="gameController">The game controller.</param>
        /// <param name="accMap">The accessibility map.</param>
        /// <param name="player">The player.</param>
        /// <param name="bot">The bot.</param>
        public BotObservation(GameController gameController, PathFinder.AccessibilityMap accMap, Player player, Bot bot) :
            base(gameController, accMap, player, bot)
        {
            _bot = bot;
        }

        /// <summary>
        /// Gets the build number of the bot.
        /// </summary>
        /// <returns>The build number of the bot.</returns>
        public int GetBotBuildNumber() => _bot.BotID;

        /// <summary>
        /// Gets the health of the bot in percentage.
        /// </summary>
        /// <returns>The health of the bot in percentage.</returns>
        public int GetBotHealthInPercent() => _bot.Health / (Bot.MaxHealth / 100);

        /// <summary>
        /// Determines whether the bot can pick up minerals.
        /// </summary>
        /// <returns><c>true</c> if the bot can pick up minerals; otherwise, <c>false</c>.</returns>
        public bool CanPickUpMinerals() => _bot.CanPickUpMinerals();
    }
}