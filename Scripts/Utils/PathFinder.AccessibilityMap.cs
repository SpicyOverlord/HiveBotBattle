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

            public AccessibilityMap(Map map, Pos startPos, BotType botType)
            {
                _map = map;
                _boolMap = FloodFill(map, startPos);
                isMinerBot = botType == BotType.MinerBot;
                isFighterBot = botType == BotType.FighterBot;
            }

            public bool Get(Pos pos) => Get(pos.X, pos.Y);

            public bool Get(int x, int y) => _boolMap[x][y];

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

                                // create test sprite
                                // map.CreateSprite(new Pos(neighborX, neighborY), CellType.Mineral);
                            }
                        }
                }

                return boolMap;
            }


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
                                if (closestDistance - distanceToTarget > 0 && startDistance - distanceToTarget > 0.5f)
                                {
                                    closestDistance = distanceToTarget;
                                    closestPos = (neighborX, neighborY);

                                    // TODO uncomment this and check if it works!
                                    // if (closestDistance < 1.5)
                                    //     return new Pos(closestPos.Item1, closestPos.Item2);
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