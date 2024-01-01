using System;
using System.Collections.Generic;
using System.Diagnostics;
using AI;
using Godot;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;

namespace HiveBotBattle.Scripts
{
    public class Player
    {
        private const bool SuppressErrors = false;

        public readonly int PlayerID;

        public readonly IHiveAI HiveAI;

        public readonly MotherShip MotherShip;
        private readonly List<Bot> _fighterBots;
        private readonly List<Bot> _minerBots;

        private int _totalBuildFighterBots;
        private int _totalBuildMinerBots;

        public bool HasLost;

        public Player(int playerID, IHiveAI hiveAI, Pos startPosition, int startMinerals)
        {
            PlayerID = playerID;
            HiveAI = hiveAI;
            MotherShip = new MotherShip(PlayerID, startPosition);

            StoredMinerals = startMinerals;

            _minerBots = new List<Bot>();
            _fighterBots = new List<Bot>();
        }

        public int StoredMinerals { get; private set; }

        public void AddMinerals(int mineralsToAdd) => StoredMinerals += mineralsToAdd;

        public int GetCurrentFighterBuildCost() => Mathf.RoundToInt((GetFighterBots().Count + 2) * GetFighterBots().Count * 0.2) + 1;

        public int GetCurrentMinerBuildCost() => Mathf.RoundToInt(GetMinerBots().Count * GetMinerBots().Count * 0.2) + 1;

        public List<Bot> GetFighterBots() => _fighterBots;

        public List<Bot> GetMinerBots() => _minerBots;

        public void TakeTurn(GameController gameController)
        {
            Map map = gameController.Map;

            if (HasLost) return;
            if (MotherShip.IsDestroyed)
            {
                HasLost = true;
                DeleteEverything(map);
                return;
            }

            PathFinder.AccessibilityMap MinerAccMap = PathFinder.CalculateAccessibilityMap(map, MotherShip.Pos, BotType.MinerBot);
            PathFinder.AccessibilityMap FighterAccMap = PathFinder.CalculateAccessibilityMap(map, MotherShip.Pos, BotType.FighterBot);


            UpdateFighterBots(gameController, map, FighterAccMap);

            UpdateMinerBot(gameController, map, MinerAccMap);

            UpdateMotherShip(gameController, map, FighterAccMap);
        }

        private void UpdateFighterBots(GameController gameController, Map map, PathFinder.AccessibilityMap fighterAccMap)
        {
            for (int i = 0; i < _fighterBots.Count; i++)
            {
                Bot fighterBot = _fighterBots[i];
                // delete bot if it is destroyed
                if (fighterBot == null || fighterBot.IsDestroyed)
                {
                    if (fighterBot != null)
                        map.DestroyVisual(fighterBot.Pos);

                    _fighterBots.RemoveAt(i--);
                    continue;
                }

                try
                {
                    FighterBotMove fighterMove =
                        HiveAI.FighterAI(new BotObservation(gameController, fighterAccMap, this, fighterBot));

                    if (fighterMove.Type is not (FighterMoveType.DoNothing or FighterMoveType.Heal) &&
                        fighterMove.TargetPos == null)
                        throw new Exception(fighterMove.Type + ": MoveTarget is null!");

                    switch (fighterMove.Type)
                    {
                        case FighterMoveType.DoNothing:
                            continue;
                        case FighterMoveType.Move:
                            if (!map.IsWalkable(fighterMove.TargetPos) || map.GetFighterBotCell(fighterMove.TargetPos) != null)
                                break;

                            map.MoveBotTo(fighterBot, fighterMove.TargetPos);
                            fighterBot.MoveTo(fighterMove.TargetPos);
                            break;
                        case FighterMoveType.MoveTowards:
                            // if the target position is not accessible, find path to closest pos
                            Pos targetPos = fighterAccMap.FindClosestPos(fighterBot.Pos, fighterMove.TargetPos);

                            //find path
                            Pos nextPos = PathFinder.FindPath(map, fighterBot.Pos, targetPos, BotType.FighterBot);

                            // do nothing if the next position on the path is the pos we are on.
                            if (fighterBot.Pos.Equals(nextPos))
                                break;

                            if (!map.IsWalkable(nextPos) || map.GetFighterBotCell(nextPos) != null)
                                break;

                            map.MoveBotTo(fighterBot, nextPos);
                            fighterBot.MoveTo(nextPos);
                            break;
                        case FighterMoveType.Shoot:
                            if (map.IsShootable(fighterMove.TargetPos) &&
                                fighterBot.Pos.InShootingRangeOf(fighterMove.TargetPos))
                            {
                                GameAgent gameAgent = gameController.GetGameAgentAt(fighterMove.TargetPos);
                                gameAgent.Damage(Bot.DamageAmount);
                            }
                            else
                            {
                                if (fighterMove.TargetPos == null)
                                    throw new Exception("Shoot failed: Target is null!");
                                if (map.GetFighterBotCell(fighterMove.TargetPos) == null)
                                    throw new Exception("Shoot failed: Cell is null! " + fighterMove.TargetPos);

                                throw new Exception("Shoot failed: Bot" + fighterBot.Pos + "  Target" +
                                                    fighterMove.TargetPos + " " +
                                                    map.GetCell(fighterMove.TargetPos).CellType.ToString());
                            }

                            break;
                        case FighterMoveType.Heal:
                            fighterBot.Heal();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    if (!SuppressErrors) throw;

                    LogError(e);
                }
            }
        }

        private void UpdateMinerBot(GameController gameController, Map map, PathFinder.AccessibilityMap minerAccMap)
        {
            for (int i = 0; i < _minerBots.Count; i++)
            {
                Bot minerBot = _minerBots[i];
                // delete bot if it is destroyed
                if (minerBot.IsDestroyed)
                {
                    _minerBots.RemoveAt(i--);
                    map.DestroyVisual(minerBot.Pos);
                    continue;
                }

                try
                {
                    MinerBotMove minerMove = HiveAI.MinerAI(new BotObservation(gameController, minerAccMap, this, minerBot));

                    if (minerMove.Type is not (MinerMoveType.DoNothing or MinerMoveType.Heal) &&
                        minerMove.TargetPos == null)
                        throw new Exception(minerMove.Type + ": MoveTarget is null!");

                    switch (minerMove.Type)
                    {
                        case MinerMoveType.DoNothing:
                            continue;
                        case MinerMoveType.Move:
                            if (!map.IsWalkable(minerMove.TargetPos) || map.GetMinerBotCell(minerMove.TargetPos) != null)
                                break;

                            map.MoveBotTo(minerBot, minerMove.TargetPos);
                            minerBot.MoveTo(minerMove.TargetPos);
                            break;
                        case MinerMoveType.MineTowards:
                        case MinerMoveType.MoveTowards:
                            bool canMine = minerMove.Type == MinerMoveType.MineTowards;

                            // if the target position is not accessible, find path to closest pos
                            // (if we can mine, every pos is accessible)
                            Pos targetPos = minerMove.TargetPos;
                            if (!canMine || map.IsSurrounded(targetPos, canMine))
                                targetPos = minerAccMap.FindClosestPos(minerBot.Pos, minerMove.TargetPos);

                            //find path
                            Pos nextPos = PathFinder.FindPath(map, minerBot.Pos, targetPos, BotType.MinerBot, canMine);

                            // do nothing if the next position on the path is the pos we are on.
                            if (minerBot.Pos.Equals(nextPos))
                                break;

                            if (map.IsMineable(nextPos) && canMine)
                            {
                                map.Mine(nextPos);
                                break;
                            }

                            if (!map.IsWalkable(nextPos) || map.GetMinerBotCell(nextPos) != null)
                                break;

                            map.MoveBotTo(minerBot, nextPos);
                            minerBot.MoveTo(nextPos);
                            break;
                        case MinerMoveType.Mine:
                            if ((map.IsStone(minerMove.TargetPos) || map.IsDeposit(minerMove.TargetPos)) &&
                                minerBot.Pos.IsNextToOrEqual(minerMove.TargetPos))
                                map.Mine(minerMove.TargetPos);
                            break;
                        case MinerMoveType.PickUpMineral:
                            if (map.IsMineral(minerMove.TargetPos) && minerBot.CanPickUpMinerals() &&
                                minerBot.Pos.IsNextToOrEqual(minerMove.TargetPos))
                            {
                                // GD.Print("Pickup at: " + minerMove.TargetPos + " from: " + minerBot.Position);
                                minerBot.AddMineral();
                                map.ClearMineral(minerMove.TargetPos);
                            }
                            else
                            {
                                // GD.Print("Mine: " + minerBot.Position + " -> " + minerMove.TargetPos);
                                // Debug.Break();
                                throw new Exception("PickUpMineral failed!");
                            }

                            break;
                        case MinerMoveType.Heal:
                            minerBot.Heal();
                            break;
                        case MinerMoveType.UnloadMinerals:
                            if (minerBot.Pos.IsNextToOrEqual(minerMove.TargetPos) && map.IsMotherShip(minerMove.TargetPos))
                            {
                                AddMinerals(minerBot.PickedUpMinerals);
                                minerBot.RemovePickedUpMinerals();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    if (!SuppressErrors) throw;

                    LogError(e);
                }
            }
        }

        private void UpdateMotherShip(GameController gameController, Map map, PathFinder.AccessibilityMap fighterAccMap)
        {
            try
            {
                MotherShipMoveType motherShipMove =
                    HiveAI.MotherShipAI(new MotherShipObservation(gameController, fighterAccMap, this, MotherShip));

                switch (motherShipMove)
                {
                    case MotherShipMoveType.DoNothing:
                        break;
                    case MotherShipMoveType.BuildFighter:
                        BuildBot(map, BotType.FighterBot, PlayerID);
                        break;
                    case MotherShipMoveType.BuildMiner:
                        BuildBot(map, BotType.MinerBot, PlayerID);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                if (!SuppressErrors) throw;

                LogError(e);
            }
        }

        private void DeleteEverything(Map map)
        {
            foreach (Bot fighterBot in _fighterBots) map.DestroyVisual(fighterBot.Pos);

            foreach (Bot minerBot in _minerBots) map.DestroyVisual(minerBot.Pos);

            map.DestroyVisual(MotherShip.Pos);
        }

        private void BuildBot(Map map, BotType botType, int playerID)
        {
            // if we can't afford, do nothing
            if (botType == BotType.FighterBot && GetCurrentFighterBuildCost() > StoredMinerals ||
                botType == BotType.MinerBot && GetCurrentMinerBuildCost() > StoredMinerals) return;

            Pos emptyNeighbor = map.GetFirstEmptyNeighbor(MotherShip.Pos);
            // if no empty neighbors, do nothing
            if (emptyNeighbor == null) return;

            if (botType == BotType.FighterBot)
            {
                // create new fighter bot at empty neighbor
                Bot newFighterBot = new Bot(PlayerID, _totalBuildFighterBots, BotType.FighterBot, emptyNeighbor);
                _fighterBots.Add(newFighterBot);
                // create visual
                map.CreateVisual(emptyNeighbor, CellType.FighterBot, playerID);

                PayForFighterBot();
                _totalBuildFighterBots++;
            }
            else if (botType == BotType.MinerBot)
            {
                // create new miner bot at empty neighbor
                Bot newMinerBot = new Bot(PlayerID, _totalBuildMinerBots, BotType.MinerBot, emptyNeighbor);
                _minerBots.Add(newMinerBot);
                // create visual
                map.CreateVisual(emptyNeighbor, CellType.MinerBot, playerID);

                PayForMinerBot();
                _totalBuildMinerBots++;
            }
        }

        public bool PayForFighterBot()
        {
            if (StoredMinerals >= GetCurrentFighterBuildCost())
            {
                StoredMinerals -= GetCurrentFighterBuildCost();
                return true;
            }

            return false;
        }

        public bool PayForMinerBot()
        {
            if (StoredMinerals >= GetCurrentMinerBuildCost())
            {
                StoredMinerals -= GetCurrentMinerBuildCost();
                return true;
            }

            return false;
        }

        private static void LogError(Exception e)
        {
            GD.Print("[ERROR] " + e.Message + "\n" + e.StackTrace);
        }
    }
}