using System.Collections.Generic;
using System.Linq;
using HiveBotBattle.Scripts;
using HiveBotBattle.Scripts.Utils.Types;


namespace Utils.Observations
{
    public abstract class Observation
    {
        private readonly GameAgent _gameAgent;
        private readonly GameController _gameController;
        private readonly Map _map;
        private readonly PathFinder.AccessibilityMap _accMap;

        protected readonly Player Player;
        private readonly int turnCount;

        public Observation(GameController gameController, PathFinder.AccessibilityMap accMap, Player player, GameAgent gameAgent)
        {
            _gameController = gameController;
            _map = gameController.Map;
            _accMap = accMap;
            Player = player;
            _gameAgent = gameAgent;
            turnCount = gameController.TurnCount;
        }

        public int GetPlayerID() => _gameAgent.PlayerID;

        public Pos GetBotPosition() => _gameAgent.Pos.Clone();

        public int GetBotHealth() => _gameAgent.Health;

        public int GetTurnCount() => turnCount;

        public bool IsNextTo(Pos pos) => _gameAgent.Pos.IsNextToOrEqual(pos);

        public float DistanceTo(Pos pos) => _gameAgent.Pos.DistanceTo(pos);

        private Map.Cell GetCell(Pos pos) => _map.GetCell(pos);

        public CellType GetCellType(Pos pos) => GetCell(pos)?.CellType ?? _map.GetMinerBotCell(pos)?.CellType ?? _map.GetFighterBotCell(pos)?.CellType ?? CellType.Empty;

        public bool IsCellType(Pos pos, CellType cellType) => _map.IsCellType(pos, cellType);

        public bool IsEmpty(int x, int y) => _map.IsEmpty(x, y);

        public bool IsEmpty(Pos pos) => _map.IsEmpty(pos);

        public bool IsMineable(Pos pos) => _map.IsMineable(pos);

        public bool IsBlocked(Pos pos, bool canMine = false) => _map.IsBlocked(pos, canMine);

        public int GetEnemyCount() => _gameController.GetEnemyMotherShips(Player.PlayerID).Count;

        public int GetFriendlyFighterBotCount() => Player.GetFighterBots().Count;

        public int GetFriendlyMinerBotCount() => Player.GetMinerBots().Count;

        public int GetEnemyFighterBotCount() => _gameController.GetEnemyFighterBots(GetPlayerID()).Count;

        public int GetEnemyMinerBotCount() => _gameController.GetEnemyMinerBots(GetPlayerID()).Count;

        public List<Pos> GetFriendlyFighterBotPositions() => Player.GetFighterBots()
                .Select(bot => bot.Pos.Clone()).ToList();

        public List<Pos> GetFriendlyMinerBotPositions() => Player.GetMinerBots()
                .Select(bot => bot.Pos.Clone()).ToList();

        public List<Pos> GetEnemyFighterBotPositions() => _gameController.GetEnemyFighterBots(GetPlayerID())
                .Select(bot => bot.Pos.Clone()).ToList();

        public List<Pos> GetEnemyMinerBotPositions() => _gameController.GetEnemyMinerBots(GetPlayerID())
                .Select(bot => bot.Pos.Clone()).ToList();


        public Pos GetNearestPosition(List<Pos> posList)
        {
            Pos nearestPos = null;
            float lowestDistance = float.MaxValue;
            foreach (Pos pos in posList)
            {
                float distance = GetBotPosition().DistanceToSquared(pos);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    nearestPos = pos;
                }
            }

            return nearestPos?.Clone();
        }

        public Pos GetNearestFriendlyFighterBotPosition() => GetNearestPosition(GetFriendlyFighterBotPositions());

        public Pos GetNearestFriendlyMinerBotPosition() => GetNearestPosition(GetFriendlyMinerBotPositions());

        public Pos GetNearestEnemyFighterBotPosition() => GetNearestPosition(GetEnemyFighterBotPositions());

        public Pos GetNearestEnemyMinerBotPosition() => GetNearestPosition(GetEnemyMinerBotPositions());

        // get Bots in range
        private List<Pos> GetPosInShootingRange(List<Pos> botList) => botList.Where(pos => _gameAgent.Pos.InShootingRangeOf(pos))
                .Select(pos => pos.Clone()).ToList();

        public List<Pos> GetEnemyFighterBotsInShootingRange() => GetPosInShootingRange(GetEnemyFighterBotPositions());

        public List<Pos> GetEnemyMinerBotsInShootingRange() => GetPosInShootingRange(GetEnemyMinerBotPositions());

        // get MotherShips
        public Pos GetFriendlyMotherShipPosition() => _gameController.GetFriendlyMotherShip(GetPlayerID()).Pos.Clone();

        public List<Pos> GetEnemyMotherShipPositions() => _gameController.GetEnemyMotherShips(GetPlayerID())
                .Select(enemyMotherShip => enemyMotherShip.Pos.Clone()).ToList();

        public Pos GetNearestEnemyMotherShipPosition() => GetNearestPosition(GetEnemyMotherShipPositions());

        // get other stuff
        public List<Pos> GetMineralPositions() => _map.GetMineralPositions().Select(pos => pos.Clone()).ToList();

        public List<Pos> GetDepositPositions() => _map.GetDepositPositions().Select(pos => pos.Clone()).ToList();

        public Pos GetNearestMineralPosition() => _map.mineralBSPTree.FindNearestPos(GetBotPosition());// return GetNearestPosition(GetMineralPositions());

        public Pos GetNearestDepositPosition() => _map.depositBSPTree.FindNearestPos(GetBotPosition());// return GetNearestPosition(GetDepositPositions());

        public int GetMapWidth() => _map.Width;

        public int GetMapHeight() => _map.Width;

        // returns true, if it is possible to path-find to a cell or one of it's neighbors.
        // If canMine is true, then the function will factor in that the path-finding can use mining to reach the target. 
        public bool CanPathFindTo(Pos pos)
        {
            if (_accMap.Get(pos.X, pos.Y))
                return true;

            for (int xDiff = -1; xDiff <= 1; xDiff++)
                for (int yDiff = -1; yDiff <= 1; yDiff++)
                {
                    if (xDiff == 0 && yDiff == 0) continue;
                    if (_accMap.Get(pos.X + xDiff, pos.Y + yDiff))
                        return true;
                }

            return false;
        }
    }
}