using HiveBotBattle.Scripts;

namespace Utils.Observations
{
    public class BotObservation : Observation
    {
        private readonly Bot _bot;


        public BotObservation(GameController gameController, PathFinder.AccessibilityMap accMap, Player player, Bot bot) :
            base(gameController, accMap, player, bot)
        {
            _bot = bot;
        }


        public int GetBotBuildNumber() => _bot.BotID;

        public int GetBotHealthInPercent() => _bot.Health / (Bot.MaxHealth / 100);

        public bool CanPickUpMinerals() => _bot.CanPickUpMinerals();
    }
}