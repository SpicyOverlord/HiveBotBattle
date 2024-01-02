using Utils;

namespace HiveBotBattle.Scripts.Utils
{
    /// <summary>
    /// Represents a node in a path for pathfinding algorithms.
    /// </summary>
    public class PathNode
    {
        /// <summary>
        /// The X coordinate of the node.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// The Y coordinate of the node.
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// The previous node in the path.
        /// </summary>
        public PathNode PreviousNode { get; private set; }

        /// <summary>
        /// The distance weight of the node.
        /// </summary>
        public float DistanceWeight { get; private set; }

        /// <summary>
        /// The ID of the node.
        /// </summary>
        public int ID => X * 10000 + Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathNode"/> class.
        /// </summary>
        /// <param name="pos">The position of the node.</param>
        /// <param name="previousNode">The previous node in the path.</param>
        /// <param name="distanceWeight">The distance weight of the node.</param>
        public PathNode(Pos pos, PathNode previousNode, float distanceWeight)
        {
            X = pos.X;
            Y = pos.Y;

            PreviousNode = previousNode;
            DistanceWeight = distanceWeight;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathNode"/> class.
        /// </summary>
        /// <param name="x">The X coordinate of the node.</param>
        /// <param name="y">The Y coordinate of the node.</param>
        /// <param name="previousNode">The previous node in the path.</param>
        /// <param name="distanceWeight">The distance weight of the node.</param>
        public PathNode(int x, int y, PathNode previousNode, float distanceWeight)
        {
            X = x;
            Y = y;

            PreviousNode = previousNode;
            DistanceWeight = distanceWeight;
        }

        /// <summary>
        /// Updates the node with a new previous node and distance to start.
        /// </summary>
        /// <param name="newPreviousNode">The new previous node in the path.</param>
        /// <param name="newDistanceToStart">The new distance to start.</param>
        public void Update(PathNode newPreviousNode, float newDistanceToStart)
        {
            PreviousNode = newPreviousNode;
            DistanceWeight = newDistanceToStart;
        }

        /// <summary>
        /// Gets the node as a position.
        /// </summary>
        /// <returns>The node as a position.</returns>
        public Pos GetAsPos()
        {
            return new Pos(X, Y);
        }
    }
}