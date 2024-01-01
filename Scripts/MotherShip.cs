using System.Diagnostics;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;

namespace HiveBotBattle.Scripts
{
    public class MotherShip : GameAgent
    {
        public const int MaxHealth = 1000;

        public MotherShip(int playerID, Pos startPosition) :
            base(playerID, BotType.MotherShip, startPosition, MaxHealth)
        { }
    }
}