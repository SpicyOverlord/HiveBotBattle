using System;
using System.Collections.Generic;
using HiveBotBattle.Scripts.Utils;
using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    public partial class PathFinder
    {
        private const int AStarMultiplier = 2;

        /// <summary>
        /// Generates an ID/Hash based on the given x and y coordinates.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The generated ID.</returns>
        private static int GenerateID(int x, int y) => x * 10000 + y;

        /// <summary>
        /// Finds the shortest path from a start position to a target position on a map for a specific bot type.
        /// </summary>
        /// <param name="map">The map on which to find the path.</param>
        /// <param name="startPos">The starting position.</param>
        /// <param name="targetPos">The target position.</param>
        /// <param name="botType">The type of bot for which to find the path. Must be either MinerBot or FighterBot.</param>
        /// <param name="canMine">Optional. If true, the bot can mine through obstacles. Default is false.</param>
        /// <returns>The next position on the shortest path from the start position to the target position. If no path is found, returns the start position.</returns>
        /// <exception cref="Exception">Thrown if botType is not MinerBot or FighterBot, or if startPos or targetPos is null, or if the pathfinding takes too long.</exception>
        public static Pos FindPath(Map map, Pos startPos, Pos targetPos, BotType botType, bool canMine = false)
        {
            if (botType is BotType.None or BotType.MotherShip)
                throw new Exception("BotType is not miner or fighter!");

            bool isMinerBot = botType == BotType.MinerBot;
            bool isFighterBot = botType == BotType.MinerBot;

            if (startPos is null)
                throw new Exception("Start pos is null!");
            if (targetPos is null)
                throw new Exception("target pos is null!");

            bool targetIsBot = map.IsShootable(targetPos);

            PriorityQueue<PathNode, float> priorityQueue = new PriorityQueue<PathNode, float>(0);
            Dictionary<int, PathNode> pathNodeDictionary = new Dictionary<int, PathNode>();
            Dictionary<int, PathNode> pathNodesInQueue = new Dictionary<int, PathNode>();

            PathNode startNode = new PathNode(startPos, null, 0);
            priorityQueue.Enqueue(startNode, 0);

            int searchCount = 0;
            PathNode currentPathNode = null;
            while (priorityQueue.Count != 0)
            {
                searchCount++;
                if (searchCount > 100000) throw new Exception("PathFinder taking too long!");

                currentPathNode = priorityQueue.Dequeue();
                pathNodesInQueue.Remove(currentPathNode.ID);

                if (targetIsBot && targetPos.IsNextToOrEqual(currentPathNode.X,currentPathNode.Y) || targetPos.Equals(currentPathNode.X,currentPathNode.Y))
                {
                    // path found!
                    break;
                }

                for (int xDiff = -1; xDiff <= 1; xDiff++)
                    for (int yDiff = -1; yDiff <= 1; yDiff++)
                    {
                        if (xDiff == 0 && yDiff == 0) continue;

                        int neighborX = currentPathNode.X + xDiff;
                        int neighborY = currentPathNode.Y + yDiff;

                        if (map.IsBlocked(neighborX, neighborY, canMine)) continue;

                        float newDistance = currentPathNode.DistanceWeight +
                                      Pos.DistanceTo(currentPathNode.X, currentPathNode.Y,
                                          neighborX, neighborY);

                        // mining takes longer than walking around
                        if (canMine && map.IsMineable(neighborX, neighborY))
                        {
                            newDistance += 1;
                        }

                        //TODO THIS DOESN'T WORK
                        // go around bots if possible, else try walking though them (basically stand still as this is not possible)
                        else if (isMinerBot && map.IsMinerBot(neighborX, neighborY) || isFighterBot && map.IsFighterBot(neighborX, neighborY))
                        {
                            // higher value for canMine, as we probably can mine around bot.
                            if (canMine)
                                newDistance += 10;
                            else
                                newDistance += 4;
                        }

                        bool valueFound =
                        pathNodeDictionary.TryGetValue(GenerateID(neighborX, neighborY), out PathNode newPathNode);
                        if (!valueFound)
                        {
                            newPathNode = new PathNode(neighborX, neighborY, currentPathNode, newDistance);
                            pathNodeDictionary.Add(newPathNode.ID, newPathNode);
                            priorityQueue.Enqueue(newPathNode,
                                newPathNode.DistanceWeight +
                                Pos.DistanceTo(neighborX, neighborY, targetPos.X, targetPos.Y) * AStarMultiplier);
                            pathNodesInQueue.Add(newPathNode.ID, newPathNode);
                        }
                        else if (newPathNode.DistanceWeight > newDistance)
                        {
                            newPathNode.Update(currentPathNode, newDistance);

                            if (!pathNodesInQueue.ContainsKey(newPathNode.ID))
                            {
                                priorityQueue.Enqueue(newPathNode,
                                    newPathNode.DistanceWeight +
                                    Pos.DistanceTo(neighborX, neighborY, targetPos.X, targetPos.Y) * AStarMultiplier);
                                pathNodesInQueue.Add(newPathNode.ID, newPathNode);
                            }
                        }
                    }
            }

            if (currentPathNode is null) throw new Exception("CurrentVertex is null!");

            while (currentPathNode.PreviousNode is not null)
            {
                if (currentPathNode.PreviousNode.PreviousNode is null) break;
                currentPathNode = currentPathNode.PreviousNode;
            }

            return new Pos(currentPathNode.X, currentPathNode.Y);
        }

        /// <summary>
        /// Generates an accessibility map that shows what cells are accessable from the start posisiton.
        /// </summary>
        /// <param name="map">The map to generate the accessibility map for.</param>
        /// <param name="startPos">The starting position for the accessibility map.</param>
        /// <param name="botType">The type of bot for which the accessibility map is generated.</param>
        /// <returns>The generated accessibility map.</returns>
        public static AccessibilityMap GenerateAccessibilityMap(Map map, Pos startPos, BotType botType) => new AccessibilityMap(map, startPos, botType);



    }
}