using Godot;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;
using Utils.Observations;

namespace HiveMind
{
    public class MinersOnly : IHiveMind
    {
        public MotherShipMove MotherShipAI(MotherShipObservation obs)
        {
            if (obs.GetFriendlyMinerBotCount() <= 100 && obs.GetMinerBuildCost() <= obs.GetStoredMinerals()){
                return new MotherShipMove(MotherShipMoveType.BuildMiner);
            }
            return new MotherShipMove(MotherShipMoveType.DoNothing);
        }

        public FighterBotMove FighterAI(BotObservation obs)
        {
            Pos nearestFighterPos = obs.GetNearestEnemyFighterBotPosition();
            // if a enemy Fighter is in range, shoot it
            if (nearestFighterPos is not null && obs.GetBotPosition().InShootingRangeOf(nearestFighterPos))
                return new FighterBotMove(FighterMoveType.Shoot, nearestFighterPos);

            Pos nearestEnemyMotherShip = obs.GetNearestEnemyMotherShipPosition();
            // if a enemy MotherShip is in range, shoot it
            if (nearestEnemyMotherShip is not null && obs.GetBotPosition().InShootingRangeOf(nearestEnemyMotherShip))
                return new FighterBotMove(FighterMoveType.Shoot, nearestEnemyMotherShip);

            Pos nearestMinerPos = obs.GetNearestEnemyMinerBotPosition();
            // if a enemy Miner is in range, shoot it
            if (nearestMinerPos is not null && obs.GetBotPosition().InShootingRangeOf(nearestMinerPos))
                return new FighterBotMove(FighterMoveType.Shoot, nearestMinerPos);

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

            Pos nearestMineral = obs.GetNearestMineralPosition();
            Pos nearestDeposit = obs.GetNearestDepositPosition();

            // GD.Print("nearestMineral: " + nearestMineral + " - nearestDeposit: " + nearestDeposit);

            // if there is nothing to mine, do nothing
            if (nearestMineral is null && nearestDeposit is null)
                return new MinerBotMove(MinerMoveType.DoNothing);

            // move towards nearest deposit if there is no minerals in the map
            if (nearestMineral is null)
                return new MinerBotMove(MinerMoveType.MineTowards, nearestDeposit);

            // move towards nearest mineral if there is no deposits in the map
            if (nearestDeposit is null)
                return new MinerBotMove(MinerMoveType.MineTowards, nearestMineral);

            // pick up mineral if bot is next to it
            if (obs.IsNextTo(nearestMineral))
                return new MinerBotMove(MinerMoveType.PickUpMineral, nearestMineral);

            // mine towards the nearest mineral, if it is closer than the nearest deposit
            if (obs.DistanceTo(nearestMineral) < obs.DistanceTo(nearestDeposit))
                return new MinerBotMove(MinerMoveType.MineTowards, nearestMineral);
            // else, mine towards the nearest deposit
            return new MinerBotMove(MinerMoveType.MineTowards, nearestDeposit);
        }
    }
}