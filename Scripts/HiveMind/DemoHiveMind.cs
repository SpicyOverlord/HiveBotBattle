using System.Collections.Generic;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;

namespace HiveMind
{
    public class DemoHiveMind : IHiveMind
    {
        public MotherShipMove MotherShipAI(MotherShipObservation obs)
        {
            bool canBuildMinerBot = obs.GetStoredMinerals() >= obs.GetMinerBuildCost();
            bool canBuildFighterBot = obs.GetStoredMinerals() >= obs.GetFighterBuildCost();

            // if we have less than 10 miners and we have enough minerals to build a miner, we build a miner
            if (obs.GetFriendlyMinerBotCount() < obs.GetMapWidth() / 5)
            {
                if (canBuildMinerBot)
                    return new MotherShipMove(MotherShipMoveType.BuildMiner);

                return new MotherShipMove(MotherShipMoveType.DoNothing);
            }

            if (canBuildFighterBot)
                return new MotherShipMove(MotherShipMoveType.BuildFighter);

            return new MotherShipMove(MotherShipMoveType.DoNothing);
        }

        public FighterBotMove FighterAI(BotObservation obs)
        {
            List<Pos> nearestFighterPos = obs.GetXNearestEnemyFighterBotPositions();
            // if a enemy Fighter is in range, shoot it
            if (nearestFighterPos is not null && obs.GetBotPosition().InShootingRangeOf(nearestFighterPos[0]))
                return new FighterBotMove(FighterMoveType.Shoot, nearestFighterPos[0]);

            Pos nearestEnemyMotherShip = obs.GetNearestEnemyMotherShipPosition();
            // if a enemy MotherShip is in range, shoot it
            if (nearestEnemyMotherShip is not null && obs.GetBotPosition().InShootingRangeOf(nearestEnemyMotherShip))
                return new FighterBotMove(FighterMoveType.Shoot, nearestEnemyMotherShip);

            List<Pos> nearestMinerPos = obs.GetXNearestEnemyMinerBotPositions();
            // if a enemy Miner is in range, shoot it
            if (nearestMinerPos is not null && obs.GetBotPosition().InShootingRangeOf(nearestMinerPos[0]))
                return new FighterBotMove(FighterMoveType.Shoot, nearestMinerPos[0]);

            // if nothing in range, move towards the nearest enemy mothership
            return new FighterBotMove(FighterMoveType.MoveTowards, nearestEnemyMotherShip);
        }

        public MinerBotMove MinerAI(BotObservation obs)
        {
            Pos motherShipPos = obs.GetFriendlyMotherShipPosition();
            // unload minerals to mothership if minder bot is full
            if (!obs.CanPickUpMinerals())
            {
                if (obs.IsNextTo(motherShipPos))
                    return new MinerBotMove(MinerMoveType.UnloadMinerals, motherShipPos);

                return new MinerBotMove(MinerMoveType.MineTowards, motherShipPos);
            }

            List<Pos> nearestMineral = obs.GetXNearestMineralPositions();
            List<Pos> nearestDeposit = obs.GetXNearestDepositPositions();

            // if there is nothing to mine, do nothing
            if (nearestMineral is null && nearestDeposit is null)
                return new MinerBotMove(MinerMoveType.DoNothing);

            // move towards nearest deposit if there is no minerals in the map
            if (nearestMineral is null)
                return new MinerBotMove(MinerMoveType.MineTowards, nearestDeposit[0]);

            // move towards nearest mineral if there is no deposits in the map
            if (nearestDeposit is null)
                return new MinerBotMove(MinerMoveType.MineTowards, nearestMineral[0]);

            // pick up mineral if bot is next to it
            if (obs.IsNextTo(nearestMineral[0]))
                return new MinerBotMove(MinerMoveType.PickUpMineral, nearestMineral[0]);

            // mine towards the nearest mineral, if it is closer than the nearest deposit
            if (obs.DistanceTo(nearestMineral[0]) < obs.DistanceTo(nearestDeposit[0]))
                return new MinerBotMove(MinerMoveType.MineTowards, nearestMineral[0]);
            // else, mine towards the nearest deposit
            return new MinerBotMove(MinerMoveType.MineTowards, nearestDeposit[0]);
        }
    }
}