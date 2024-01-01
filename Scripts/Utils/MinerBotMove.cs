using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    public readonly struct MinerBotMove
    {
        public readonly bool HasTarget;
        public readonly Pos TargetPos;
        public readonly MinerMoveType Type;

        public MinerBotMove(MinerMoveType minerMoveType, Pos targetPos)
        {
            Type = minerMoveType;
            TargetPos = targetPos;
            HasTarget = true;
        }

        public MinerBotMove(MinerMoveType minerMoveType, int x, int y)
        {
            Type = minerMoveType;
            TargetPos = new Pos(x, y);
            HasTarget = true;
        }

        public MinerBotMove(MinerMoveType minerMoveType)
        {
            Type = minerMoveType;
            TargetPos = null;
            HasTarget = false;
        }
    }
}