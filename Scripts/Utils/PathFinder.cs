using System;
using System.Collections.Generic;
using Godot;
using HiveBotBattle.Scripts.Utils;
using HiveBotBattle.Scripts.Utils.Types;
// using DataStructures.PriorityQueue;

namespace Utils
{
    public partial class PathFinder
    {
        private const int AStarMultiplier = 2;

        private static int GetID(int x, int y)
        {
            return x * 10000 + y;
        }

        public static Pos FindPath(Map map, Pos startPos, Pos targetPos, BotType botType, bool canMine = false)
        {
            if (botType is BotType.None or BotType.MotherShip)
                throw new Exception("BotType is not miner or fighter!");

            bool isMinerBot = botType == BotType.MinerBot;
            bool isFighterBot = botType == BotType.MinerBot;

            bool pathFound = false;

            if (startPos == null)
                throw new Exception("Start pos is null!");
            if (targetPos == null)
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

                // create test sprite
                // map.CreateSprite(currentPathNode.GetAsPos(), CellType.Mineral);

                if (targetIsBot && targetPos.IsNextToOrEqual(currentPathNode.X,currentPathNode.Y) || targetPos.Equals(currentPathNode.X,currentPathNode.Y))
                {
                    pathFound = true;
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
                        pathNodeDictionary.TryGetValue(GetID(neighborX, neighborY), out PathNode newPathNode);
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

            // if (!pathFound) throw new Exception ("NO PATH FOUND!");
            if (!pathFound)
            {
                // GD.Print(startPos + " -> " + targetPos);
                // Debug.Break();
            }
            // if (!pathFound) return startPos;

            if (currentPathNode == null) throw new Exception("CurrentVertex == null!");

            while (currentPathNode.PreviousNode != null)
            {
                if (currentPathNode.PreviousNode.PreviousNode == null) break;
                currentPathNode = currentPathNode.PreviousNode;
            }

            return new Pos(currentPathNode.X, currentPathNode.Y);
        }

        public static AccessibilityMap CalculateAccessibilityMap(Map map, Pos startPos, BotType botType)
        {
            return new AccessibilityMap(map, startPos, botType);
        }


        
    }
}