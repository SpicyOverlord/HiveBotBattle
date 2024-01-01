using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;

namespace AI
{
    public interface IHiveAI
    {
        public MotherShipMoveType MotherShipAI(MotherShipObservation obs);
        public FighterBotMove FighterAI(BotObservation obs);
        public MinerBotMove MinerAI(BotObservation obs);
    }
}