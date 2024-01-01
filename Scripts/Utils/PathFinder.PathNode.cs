using Utils;

namespace HiveBotBattle.Scripts.Utils
{
    public class PathNode
    {
        public readonly int X;
        public readonly int Y;

        public PathNode PreviousNode { get; private set; }
        public float DistanceWeight { get; private set; }
        public int ID => X * 10000 + Y;

        public PathNode(Pos pos, PathNode previousNode, float distanceWeight)
        {
            X = pos.X;
            Y = pos.Y;

            PreviousNode = previousNode;
            DistanceWeight = distanceWeight;
        }

        public PathNode(int x, int y, PathNode previousNode, float distanceWeight)
        {
            X = x;
            Y = y;

            PreviousNode = previousNode;
            DistanceWeight = distanceWeight;
        }

        public void Update(PathNode newPreviousNode, float newDistanceToStart)
        {
            PreviousNode = newPreviousNode;
            DistanceWeight = newDistanceToStart;
        }

        public Pos GetAsPos()
        {
            return new Pos(X, Y);
        }
    }
}