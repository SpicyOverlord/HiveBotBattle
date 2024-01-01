using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;

namespace AI
{
    public class EmptyBot : IHiveAI
    {
        public MotherShipMoveType MotherShipAI(MotherShipObservation obs)
        {
            return MotherShipMoveType.DoNothing;
        }

        public FighterBotMove FighterAI(BotObservation obs)
        {
            return new FighterBotMove(FighterMoveType.DoNothing);
        }

        public MinerBotMove MinerAI(BotObservation obs)
        {
            return new MinerBotMove(MinerMoveType.DoNothing);
        }
    }
}