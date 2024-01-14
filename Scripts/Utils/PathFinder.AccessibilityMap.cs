using System.Collections.Generic;
using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    public partial class PathFinder
    {
        public class AccessibilityMap
        {
            private readonly bool[][] _boolMap;
            private readonly Map _map;

            private readonly bool isMinerBot;
            private readonly bool isFighterBot;

            /// <summary>
            /// Initializes an accessibility map used by the PathFinder class to determine reachable positions on a map.
            /// </summary>
            /// <param name="map">The map on which the accessibility map is based.</param>
            /// <param name="startPos">The starting position for the accessibility map.</param>
            /// <param name="botType">The type of bot for which the accessibility map is created.</param>
            public AccessibilityMap(Map map, Pos startPos, AgentType botType)
            {
                _map = map;
                _boolMap = FloodFill(map, startPos);
                isMinerBot = botType == AgentType.MinerBot;
                isFighterBot = botType == AgentType.FighterBot;
            }

            /// <summary>
            /// Gets the accessibility value at the specified position.
            /// </summary>
            /// <param name="pos">The position to get the accessibility value for.</param>
            /// <returns>The accessibility value at the specified position.</returns>
            public bool Get(Pos pos) => Get(pos.X, pos.Y);

            /// <summary>
            /// Gets the accessibility value at the specified coordinates.
            /// </summary>
            /// <param name="x">The x-coordinate.</param>
            /// <param name="y">The y-coordinate.</param>
            /// <returns>The accessibility value at the specified coordinates.</returns>
            public bool Get(int x, int y) => _boolMap[x][y];

            /// <summary>
            /// Performs the flood fill algorithm on the given map starting from the specified position.
            /// Determines the accessibility of each cell on the map, from a start position.
            /// </summary>
            /// <param name="map">The map to perform the flood fill on.</param>
            /// <param name="startPos">The starting position for the flood fill.</param>
            /// <returns>A bool map (2D array) indicating the accessibility of each cell on the map.</returns>
            private static bool[][] FloodFill(Map map, Pos startPos)
            {
                bool[][] boolMap = new bool[map.Width][];
                for (int i = 0; i < map.Width; i++)
                    boolMap[i] = new bool[map.Height];

                Queue<(int, int)> posQueue = new Queue<(int, int)>();
                posQueue.Enqueue((startPos.X, startPos.Y));

                if (map.IsSurrounded(startPos))
                {
                    posQueue.Enqueue((startPos.X, startPos.Y + 2));
                    posQueue.Enqueue((startPos.X + 2, startPos.Y + 2));
                    posQueue.Enqueue((startPos.X + 2, startPos.Y));
                    posQueue.Enqueue((startPos.X + 2, startPos.Y - 2));
                    posQueue.Enqueue((startPos.X, startPos.Y - 2));
                    posQueue.Enqueue((startPos.X - 2, startPos.Y - 2));
                    posQueue.Enqueue((startPos.X - 2, startPos.Y));
                    posQueue.Enqueue((startPos.X - 2, startPos.Y + 2));
                }
                else
                {
                    // set mothership to accessible if it is not surrounded
                    boolMap[startPos.X][startPos.Y] = true;
                }

                while (posQueue.Count != 0)
                {
                    (int, int) currentPos = posQueue.Dequeue();

                    for (int yDiff = 1; yDiff >= -1; yDiff--)
                        for (int xDiff = -1; xDiff <= 1; xDiff++)
                        {
                            if (xDiff == 0 && yDiff == 0) continue;

                            int neighborX = currentPos.Item1 + xDiff;
                            int neighborY = currentPos.Item2 + yDiff;

                            if (!boolMap[neighborX][neighborY] && !map.IsBlocked(neighborX, neighborY))
                            {
                                boolMap[neighborX][neighborY] = true;
                                posQueue.Enqueue((neighborX, neighborY));
                            }
                        }
                }

                return boolMap;
            }


            /// <summary>
            /// Finds the closest accessible position to the target position from the start position.
            /// </summary>
            /// <param name="startPos">The starting position.</param>
            /// <param name="targetPos">The target position.</param>
            /// <returns>The closest accessible position to the target. If the target position is accessible, returns the target position.</returns>
            public Pos FindClosestPos(Pos startPos, Pos targetPos)
            {
                if (Get(targetPos))
                    return targetPos;

                for (int xDiff = -1; xDiff <= 1; xDiff++)
                    for (int yDiff = -1; yDiff <= 1; yDiff++)
                    {
                        if (xDiff == 0 && yDiff == 0) continue;

                        if (Get(targetPos.X + xDiff, targetPos.Y + yDiff))
                            return new Pos(targetPos.X + xDiff, targetPos.Y + yDiff);
                    }

                bool[][] visited = new bool[_map.Width][];
                for (int i = 0; i < _map.Width; i++)
                    visited[i] = new bool[_map.Height];

                Queue<(int, int)> posQueue = new Queue<(int, int)>();
                posQueue.Enqueue((startPos.X, startPos.Y));
                visited[startPos.X][startPos.Y] = true;


                (int, int) closestPos = (startPos.X, startPos.Y);
                float startDistance = startPos.DistanceTo(targetPos);
                float closestDistance = startDistance;

                while (posQueue.Count != 0)
                {
                    (int, int) currentPos = posQueue.Dequeue();

                    for (int yDiff = 1; yDiff >= -1; yDiff--)
                        for (int xDiff = -1; xDiff <= 1; xDiff++)
                        {
                            if (xDiff == 0 && yDiff == 0) continue;

                            int neighborX = currentPos.Item1 + xDiff;
                            int neighborY = currentPos.Item2 + yDiff;

                            if (_boolMap[neighborX][neighborY] && !visited[neighborX][neighborY] && (isMinerBot && !_map.IsMinerBot(neighborX, neighborY) || isFighterBot && !_map.IsFighterBot(neighborX, neighborY)))
                            {
                                visited[neighborX][neighborY] = true;

                                float distanceToTarget = Pos.DistanceTo(neighborX, neighborY, targetPos.X, targetPos.Y);
                                // the error margin of 0.5 is to stop the fighter bots from jittering
                                // the bots jitter between two positions because the distance to the target is the same
                                if (closestDistance - distanceToTarget > 0 && startDistance - distanceToTarget > 0.5f)
                                {
                                    closestDistance = distanceToTarget;
                                    closestPos = (neighborX, neighborY);
                                }

                                posQueue.Enqueue((neighborX, neighborY));
                            }
                        }
                }

                return new Pos(closestPos.Item1, closestPos.Item2);
            }
        }
    }
}