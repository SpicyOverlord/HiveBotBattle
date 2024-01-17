using System;
using System.Collections.Generic;
using HiveMind;
using Godot;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;
using System.Linq;
using static Map;

namespace HiveBotBattle.Scripts
{
    public class Player
    {
        private const bool SuppressErrors = false;

        public readonly int PlayerID;
        public readonly string Name;

        public readonly IHiveMind HiveMind;

        public readonly MotherShip MotherShip;
        private readonly List<Bot> _fighterBots;
        private readonly List<Bot> _minerBots;

        private int _totalBuildFighterBots;
        private int _totalBuildMinerBots;

        public bool HasLost { get; private set; }

        public Player(int playerID, string name, IHiveMind hiveMind, Pos startPosition, int startMinerals)
        {
            PlayerID = playerID;
            Name = name;
            HiveMind = hiveMind;
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

            DestroyEverythingIfLost(map);
            if (HasLost) return;

            PathFinder.AccessibilityMap MinerAccMap = PathFinder.GenerateAccessibilityMap(map, MotherShip.Pos, AgentType.MinerBot);
            PathFinder.AccessibilityMap FighterAccMap = PathFinder.GenerateAccessibilityMap(map, MotherShip.Pos, AgentType.FighterBot);

            UpdateFighterBots(gameController, map, FighterAccMap);
            UpdateMinerBot(gameController, map, MinerAccMap);
            UpdateMotherShip(gameController, map, FighterAccMap);
        }

        private void UpdateFighterBots(GameController gameController, Map map, PathFinder.AccessibilityMap fighterAccMap)
        {
            for (int i = 0; i < _fighterBots.Count; i++)
            {
                DestroyEverythingIfLost(map);
                if (HasLost || gameController.GameOver) return;

                Bot fighterBot = _fighterBots[i];
                // delete bot if it is destroyed
                if (fighterBot is null || fighterBot.ShouldBeDestroyed)
                {
                    if (fighterBot is not null)
                        map.DestroyVisual(fighterBot.Pos);

                    _fighterBots.RemoveAt(i--);
                    continue;
                }

                try
                {
                    BotObservation fighterBotObservation = new BotObservation(gameController, fighterAccMap, this, fighterBot);
                    FighterBotMove fighterMove = HiveMind.FighterAI(fighterBotObservation);

                    if (fighterMove.Type is not (FighterMoveType.DoNothing or FighterMoveType.Heal) &&
                        fighterMove.TargetPos is null)
                        throw new Exception(fighterMove.Type + ": TargetPos is null!");

                    switch (fighterMove.Type)
                    {
                        case FighterMoveType.DoNothing:
                            continue;
                        case FighterMoveType.Move:
                            if (!map.IsWalkable(fighterMove.TargetPos) || map.GetFighterBotCell(fighterMove.TargetPos) is not null)
                                break;
                            map.MoveBotTo(fighterBot, fighterMove.TargetPos);
                            break;
                        case FighterMoveType.MoveTowards:
                            // if the target position is not accessible, find path to closest pos
                            Pos targetPos = fighterAccMap.FindClosestPos(fighterBot.Pos, fighterMove.TargetPos);

                            //find path
                            Pos nextPos = PathFinder.FindPath(map, fighterBot.Pos, targetPos, AgentType.FighterBot);

                            // do nothing if the next position on the path is the pos we are on.
                            if (fighterBot.Pos.Equals(nextPos))
                                break;

                            if (!map.IsWalkable(nextPos) || map.GetFighterBotCell(nextPos) is not null)
                                break;

                            map.MoveBotTo(fighterBot, nextPos);

                            break;
                        case FighterMoveType.Shoot:
                            if (map.IsShootable(fighterMove.TargetPos) &&
                                fighterBot.Pos.InShootingRangeOf(fighterMove.TargetPos))
                            {
                                // check randomly if the bot is hit
                                if (GD.Randi() % 100 < Bot.HitChange * 100)
                                    break;

                                GameAgent gameAgentToDamage = gameController.GetGameAgentAt(fighterMove.TargetPos);
                                gameAgentToDamage.Damage();
                            }
                            else
                            {
                                if (fighterMove.TargetPos is null)
                                    throw new Exception("Shoot failed: Target is null!");
                                if (map.GetFighterBotCell(fighterMove.TargetPos) is null)
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
                DestroyEverythingIfLost(map);
                if (HasLost || gameController.GameOver) return;

                Bot minerBot = _minerBots[i];
                // delete bot if it is destroyed
                if (minerBot.ShouldBeDestroyed)
                {
                    _minerBots.RemoveAt(i--);
                    map.DestroyVisual(minerBot.Pos);
                    continue;
                }

                try
                {
                    BotObservation minerBotObservation = new BotObservation(gameController, minerAccMap, this, minerBot);
                    MinerBotMove minerMove = HiveMind.MinerAI(minerBotObservation);

                    if (minerMove.Type is not (MinerMoveType.DoNothing or MinerMoveType.Heal) &&
                        minerMove.TargetPos is null)
                        throw new Exception(minerMove.Type + ": MoveTarget is null!");

                    switch (minerMove.Type)
                    {
                        case MinerMoveType.DoNothing:
                            continue;
                        case MinerMoveType.Move:
                            if (!map.IsWalkable(minerMove.TargetPos) || map.IsMinerBot(minerMove.TargetPos))
                                break;

                            map.MoveBotTo(minerBot, minerMove.TargetPos);
                            break;
                        case MinerMoveType.MineTowards:
                        case MinerMoveType.MoveTowards:
                            bool canMine = minerMove.Type == MinerMoveType.MineTowards;

                            // if the target position is not accessible, find path to closest pos
                            // (if we can mine, every pos is accessible)
                            Pos targetPos = minerMove.TargetPos;
                            if (map.IsSurrounded(targetPos, canMine))
                                targetPos = minerAccMap.FindClosestPos(minerBot.Pos, minerMove.TargetPos);

                            //find path
                            Pos nextPos = PathFinder.FindPath(map, minerBot.Pos, targetPos, AgentType.MinerBot, canMine);

                            // do nothing if the next position on the path is the pos we are on.
                            if (minerBot.Pos.Equals(nextPos))
                                break;

                            if (map.IsMineable(nextPos))
                            {
                                map.Mine(nextPos);
                                break;
                            }

                            if (!map.IsWalkable(nextPos) || map.IsMinerBot(nextPos))
                                break;

                            if (minerBot.Pos.Equals(map.GetMinerBotCell(minerBot.Pos)?.GetPosition()))
                            {
                                GD.Print("MinerBotPos: " + minerBot.Pos + " CellPos: " + map.GetMinerBotCell(minerBot.Pos).GetPosition());
                            }

                            map.MoveBotTo(minerBot, nextPos);
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
            DestroyEverythingIfLost(map);
            if (HasLost || gameController.GameOver) return;

            try
            {
                MotherShipObservation motherShipObservation = new MotherShipObservation(gameController, fighterAccMap, this, MotherShip);
                MotherShipMove motherShipMove = HiveMind.MotherShipAI(motherShipObservation);

                switch (motherShipMove.Type)
                {
                    case MotherShipMoveType.DoNothing:
                        break;
                    case MotherShipMoveType.BuildFighter:
                        BuildBot(map, AgentType.FighterBot, PlayerID);
                        break;
                    case MotherShipMoveType.BuildMiner:
                        BuildBot(map, AgentType.MinerBot, PlayerID);
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
            for (int i = 0; i < _fighterBots.Count; i++)
            {
                Bot fighterBot = _fighterBots[i];
                fighterBot.Damage(9999999);
                map.DestroyVisual(fighterBot.Pos);
            }

            for (int i = 0; i < _minerBots.Count; i++)
            {
                Bot minerBot = _minerBots[i];
                minerBot.Damage(9999999);
                map.DestroyVisual(minerBot.Pos);
            }

            map.DestroyVisual(MotherShip.Pos);
        }

        private void BuildBot(Map map, AgentType botType, int playerID)
        {
            // if we can't afford, do nothing
            if (botType == AgentType.FighterBot && GetCurrentFighterBuildCost() > StoredMinerals ||
                botType == AgentType.MinerBot && GetCurrentMinerBuildCost() > StoredMinerals) return;

            Pos emptyNeighbor = map.GetFirstEmptyNeighbor(MotherShip.Pos);
            // if no empty neighbors, do nothing
            if (emptyNeighbor is null) return;

            if (botType == AgentType.FighterBot)
            {
                // create new fighter bot at empty neighbor
                Bot newFighterBot = new Bot(PlayerID, _totalBuildFighterBots, AgentType.FighterBot, emptyNeighbor);
                _fighterBots.Add(newFighterBot);
                // create visual
                map.CreateCell(emptyNeighbor, CellType.FighterBot, playerID, newFighterBot);

                PayForFighterBot();
                _totalBuildFighterBots++;
            }
            else if (botType == AgentType.MinerBot)
            {
                // create new miner bot at empty neighbor
                Bot newMinerBot = new Bot(PlayerID, _totalBuildMinerBots, AgentType.MinerBot, emptyNeighbor);
                _minerBots.Add(newMinerBot);
                // create visual
                map.CreateCell(emptyNeighbor, CellType.MinerBot, playerID, newMinerBot);

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

        public void DestroyEverythingIfLost(Map map)
        {
            if (!HasLost && MotherShip.ShouldBeDestroyed)
            {
                HasLost = true;
                DeleteEverything(map);
            }
        }
        private static void LogError(Exception e)
        {
            GD.Print("[ERROR] " + e.Message + "\n" + e.StackTrace);
        }
    }
}