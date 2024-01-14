using System.Collections.Generic;
using Godot;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;

namespace HiveMind
{
    public class MasterMind : IHiveMind
    {
        private bool _isOpenFight;
        private List<Pos> _knownFightTargets;
        private HashSet<Pos> _knownMineTargets;

        private Pos _nearestEnemyFighterToBase;

        public MotherShipMove MotherShipAI(MotherShipObservation obs)
        {
            Reset(obs);

            int maxMinerBots = Mathf.Min((4 - obs.GetOpponentCount()) * 5, 15) + 2 * obs.GetMapWidth() / 100;

            bool weHaveEnoughMiners = obs.GetFriendlyMinerBotCount() < maxMinerBots;
            bool weShouldBuildMiner = obs.GetDepositPositions().Count != 0 && weHaveEnoughMiners;

            bool enemyFightersTooClose = false;
            if (_nearestEnemyFighterToBase is not null)
            {
                float nearestEnemyFighterDistance = obs.GetBotPosition().DistanceTo(_nearestEnemyFighterToBase);
                float maxDistance = obs.GetBotPosition()
                    .DistanceTo(new Pos(obs.GetMapHeight() - 2, obs.GetMapWidth() - 2));
                enemyFightersTooClose = nearestEnemyFighterDistance > maxDistance * 0.3;
            }


            if ((!_isOpenFight || !enemyFightersTooClose) && weShouldBuildMiner) 
                return new MotherShipMove(MotherShipMoveType.BuildMiner);

            // if (obs.GetEnemyCount() == 1)
            //     if (obs.GetFriendlyFighterBotCount() < obs.GetEnemyFighterBotCount())
            //         return MotherShipMoveType.BuildFighter;
            //     else
            //         return MotherShipMoveType.DoNothing;
            return new MotherShipMove(MotherShipMoveType.BuildFighter);

            // return MotherShipMoveType.DoNothing;
        }

        public FighterBotMove FighterAI(BotObservation obs)
        {
            Pos botPos = obs.GetBotPosition();

            foreach (Pos knownTarget in _knownFightTargets)
                if (botPos.InShootingRangeOf(knownTarget))
                    return new FighterBotMove(FighterMoveType.Shoot, knownTarget);

            Pos target = null;

            List<Pos> enemyFighterPosList = obs.GetEnemyFighterBotsInShootingRange();
            foreach (Pos enemyFighterPos in enemyFighterPosList)
                if (_knownFightTargets.Contains(enemyFighterPos) &&
                    obs.GetCellType(enemyFighterPos) == CellType.FighterBot)
                {
                    target = enemyFighterPos;
                    break;
                }


            Pos nearestFighterPos = obs.GetNearestEnemyFighterBotPosition();
            // if an enemy Fighter is in range, shoot it
            if (target is null && nearestFighterPos is not null &&
                obs.GetBotPosition().InShootingRangeOf(nearestFighterPos))
                return new FighterBotMove(FighterMoveType.Shoot, nearestFighterPos);

            Pos nearestEnemyMotherShip = obs.GetNearestEnemyMotherShipPosition();
            // if an enemy MotherShip is in range, shoot it
            if (target is null && nearestEnemyMotherShip is not null && botPos.InShootingRangeOf(nearestEnemyMotherShip))
                target = nearestEnemyMotherShip;

            Pos nearestMinerPos = obs.GetNearestEnemyMinerBotPosition();
            // if an enemy Miner is in range, shoot it
            if (target is null && nearestMinerPos is not null && botPos.InShootingRangeOf(nearestMinerPos))
                target = nearestMinerPos;


            if (target is not null)
            {
                if (!target.Equals(nearestEnemyMotherShip))
                    _knownFightTargets.Add(target);

                return new FighterBotMove(FighterMoveType.Shoot, target);
            }

            // if (obs.GetBotID() % 3 == 0) {
            //     Pos motherShipPos = obs.GetFriendlyMotherShipPosition();
            //     float currentDistance = botPos.DistanceTo(motherShipPos);
            //     const float optimalDefendingDistance = 3;
            //
            //     if (currentDistance <= optimalDefendingDistance) {
            //         foreach (var neighborPos in botPos.GetNeighbors()) {
            //             if (neighborPos.DistanceTo(motherShipPos) > currentDistance)
            //                 return new FighterBotMove(FighterMoveType.Move, neighborPos);
            //         }
            //     }
            //     else {
            //         foreach (var neighborPos in botPos.GetNeighbors()) {
            //             if (neighborPos.DistanceTo(motherShipPos) <= currentDistance)
            //                 return new FighterBotMove(FighterMoveType.Move, neighborPos);
            //         }
            //     }
            //     
            //     return new FighterBotMove(FighterMoveType.DoNothing);
            // }
            //

            bool isMinerHunter = obs.GetBotBuildNumber() % 10 == 0;
            if (isMinerHunter && nearestMinerPos is not null)
                return new FighterBotMove(FighterMoveType.MoveTowards, nearestMinerPos);


            if (nearestFighterPos is not null &&
                obs.CanPathFindTo(nearestFighterPos) &&
                botPos.DistanceTo(nearestFighterPos) < botPos.DistanceTo(nearestEnemyMotherShip))
                return new FighterBotMove(FighterMoveType.MoveTowards, nearestFighterPos);

            if (obs.GetBotHealthInPercent() < 100)
                return new FighterBotMove(FighterMoveType.Heal);

            // if (obs.GetFriendlyFighterBotPositions().Count > 5)
            //     return new FighterBotMove(FighterMoveType.MoveTowards, nearestEnemyMotherShip);

            return new FighterBotMove(FighterMoveType.MoveTowards, nearestEnemyMotherShip);
        }

        public MinerBotMove MinerAI(BotObservation obs)
        {
            Pos botPos = obs.GetBotPosition();
            Pos motherShipPos = obs.GetFriendlyMotherShipPosition();

            // unload minerals to mothership if miner bot is full
            if (!obs.CanPickUpMinerals())
            {
                if (obs.IsNextTo(motherShipPos))
                    return new MinerBotMove(MinerMoveType.UnloadMinerals, motherShipPos);

                return new MinerBotMove(MinerMoveType.MineTowards, motherShipPos);
            }

            // pick up mineral if bot is next to it
            Pos nearMin = obs.GetNearestMineralPosition();
            if (nearMin is not null && obs.IsNextTo(nearMin))
                return new MinerBotMove(MinerMoveType.PickUpMineral, nearMin);

            // if (obs.GetDepositPositions().Count == 0 || obs.GetBotBuildNumber() % 20 == 5)
            if (obs.GetDepositPositions().Count == 0)
                return new MinerBotMove(MinerMoveType.MineTowards, obs.GetNearestEnemyMotherShipPosition());

            Pos nearestMineral = GetNearestNotAlreadyTargetedPos(botPos, obs.GetMineralPositions());
            Pos nearestDeposit = GetNearestNotAlreadyTargetedPos(botPos, obs.GetDepositPositions());

            // move towards nearest deposit if there is no minerals in the map
            if (nearestMineral is null)
                return new MinerBotMove(MinerMoveType.MineTowards, nearestDeposit);

            // move towards nearest mineral if there is no deposits in the map
            if (nearestDeposit is null)
                return new MinerBotMove(MinerMoveType.MineTowards, nearestMineral);


            // mine towards the nearest mineral, if it is closer than the nearest deposit
            if (obs.DistanceTo(nearestMineral) < obs.DistanceTo(nearestDeposit))
            {
                _knownMineTargets.Add(nearestMineral);
                return new MinerBotMove(MinerMoveType.MineTowards, nearestMineral);
            }

            // else, mine towards the nearest deposit
            _knownMineTargets.Add(nearestDeposit);
            return new MinerBotMove(MinerMoveType.MineTowards, nearestDeposit);
        }

        private void Reset(MotherShipObservation obs)
        {
            _nearestEnemyFighterToBase = obs.GetNearestEnemyFighterBotPosition();

            _knownFightTargets = new List<Pos>();
            _knownMineTargets = new HashSet<Pos>();

            _isOpenFight = false;
            foreach (Pos enemyMotherShipPos in obs.GetEnemyMotherShipPositions())
                if (obs.CanPathFindTo(enemyMotherShipPos))
                {
                    _isOpenFight = true;
                    break;
                }
        }

        public Pos GetNearestNotAlreadyTargetedPos(Pos botPos, List<Pos> posList)
        {
            Pos nearestPos = null;
            float lowestDistance = 99999;
            foreach (Pos pos in posList)
            {
                if (_knownMineTargets.Contains(pos))
                    continue;

                float distance = botPos.DistanceToSquared(pos);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    nearestPos = pos;
                }
            }

            return nearestPos;
        }
    }
}