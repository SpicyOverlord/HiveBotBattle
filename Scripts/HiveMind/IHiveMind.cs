using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;

namespace HiveMind
{
    public interface IHiveMind
    {
        public MotherShipMove MotherShipAI(MotherShipObservation obs);
        public FighterBotMove FighterAI(BotObservation obs);
        public MinerBotMove MinerAI(BotObservation obs);
    }
}