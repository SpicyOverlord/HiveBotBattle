using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    public readonly struct FighterBotMove
    {
        public readonly FighterMoveType Type;
        public readonly Pos TargetPos;
        public readonly bool HasTarget;

        public FighterBotMove(FighterMoveType fighterMoveType, Pos targetPos)
        {
            Type = fighterMoveType;
            TargetPos = targetPos;
            HasTarget = true;
        }

        public FighterBotMove(FighterMoveType fighterMoveType, int x, int y)
        {
            Type = fighterMoveType;
            TargetPos = new Pos(x, y);
            HasTarget = true;
        }

        public FighterBotMove(FighterMoveType fighterMoveType)
        {
            Type = fighterMoveType;
            TargetPos = null;
            HasTarget = false;
        }
    }
}