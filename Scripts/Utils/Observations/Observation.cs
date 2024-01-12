using System;
using System.Collections.Generic;
using System.Linq;
using HiveBotBattle.Scripts;
using HiveBotBattle.Scripts.Utils.Types;
using static Map;

namespace Utils.Observations
{
    /// <summary>
    /// Represents an observation in the game.
    /// An observation is a collection of data about the current game state,
    /// from the perspective of a bot.
    /// </summary>
    public abstract class Observation
    {
        private readonly GameAgent _gameAgent;
        private readonly GameController _gameController;
        private readonly Map _map;
        private readonly PathFinder.AccessibilityMap _accMap;

        protected readonly Player Player;
        private readonly int turnCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Observation"/> class.
        /// </summary>
        /// <param name="gameController">The game controller.</param>
        /// <param name="accMap">The accessibility map.</param>
        /// <param name="player">The player.</param>
        /// <param name="gameAgent">The gameAgent that is associated with this observation.</param>
        public Observation(GameController gameController, PathFinder.AccessibilityMap accMap, Player player, GameAgent gameAgent)
        {
            _gameController = gameController;
            _map = gameController.Map;
            _accMap = accMap;
            Player = player;
            _gameAgent = gameAgent;
            turnCount = gameController.TurnCount;

        }

        /// <summary>
        /// Gets the ID of the player associated with this observation and game agent.
        /// </summary>
        /// <returns>The player's ID.</returns>
        public int GetPlayerID() => _gameAgent.PlayerID;

        /// <summary>
        /// Gets the position of the bot associated with this observation.
        /// </summary>
        /// <returns>The bot position.</returns>
        public Pos GetBotPosition() => _gameAgent.Pos.Clone();

        /// <summary>
        /// Gets the health of the bot associated with this observation.
        /// </summary>
        /// <returns>The bot health.</returns>
        public int GetBotHealth() => _gameAgent.Health;

        public int GetTurnCount() => turnCount;

        /// <summary>
        /// Checks if the specified position is next to the bot associated with this observation.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>True if the position is next to the bot, false otherwise.</returns>
        public bool IsNextTo(Pos pos) => _gameAgent.Pos.IsNextToOrEqual(pos);

        /// <summary>
        /// Calculates the distance between the bot associated with this observation and the specified position.
        /// </summary>
        /// <param name="pos">The position to calculate the distance to.</param>
        /// <returns>The distance between the bot and the position.</returns>
        public float DistanceTo(Pos pos) => _gameAgent.Pos.DistanceTo(pos);

        private Map.Cell GetCell(Pos pos) => _map.GetCell(pos);

        public CellType GetCellType(Pos pos) => GetCell(pos)?.CellType ?? _map.GetMinerBotCell(pos)?.CellType ?? _map.GetFighterBotCell(pos)?.CellType ?? CellType.Empty;

        /// <summary>
        /// Checks if the specified position is of the specified cell type.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <param name="cellType">The cell type to compare against.</param>
        /// <returns>True if the position has the specified cell type, false otherwise.</returns>
        public bool IsCellType(Pos pos, CellType cellType) => _map.IsCellType(pos, cellType);

        /// <summary>
        /// Checks if a specific position on the map is empty.
        /// </summary>
        /// <param name="x">The x-coordinate of the position.</param>
        /// <param name="y">The y-coordinate of the position.</param>
        /// <returns>True if the position is empty, false otherwise.</returns>
        public bool IsEmpty(int x, int y) => _map.IsEmpty(x, y);

        /// <summary>
        /// Checks if a specific position on the map is empty.
        /// </summary>
        /// <param name="x">The x-coordinate of the position.</param>
        /// <param name="y">The y-coordinate of the position.</param>
        /// <returns>True if the position is empty, false otherwise.</returns>
        public bool IsEmpty(Pos pos) => _map.IsEmpty(pos);

        /// <summary>
        /// Checks if the specified position is mineable by a miner.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>True if the position is mineable, Falsle otherwise.</returns>
        public bool IsMineable(Pos pos) => _map.IsMineable(pos);

        /// <summary>
        /// Checks if a position is blocked on the map.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <param name="canMine">Optional parameter to indicate if mining is allowed.</param>
        /// <returns>True if the position is blocked and the bot cannot walk over it, False otherwise.</returns>
        public bool IsBlocked(Pos pos, bool canMine = false) => _map.IsBlocked(pos, canMine);


        /// <summary>
        /// Returns the number of alive opponents in the game.
        /// </summary>
        /// <returns>Returns the number of alive opponents in the game.</returns>
        public int GetOpponentCount() => _gameController.GetEnemyMotherShips(Player.PlayerID).Count;

        /// <summary>
        /// Gets the count of friendly fighter bots in the observation.
        /// </summary>
        /// <returns>The count of friendly fighter bots.</returns>
        public int GetFriendlyFighterBotCount() => Player.GetFighterBots().Count;

        /// <summary>
        /// Gets the count of friendly miner bots.
        /// </summary>
        /// <returns>The count of friendly miner bots.</returns>
        public int GetFriendlyMinerBotCount() => Player.GetMinerBots().Count;

        /// <summary>
        /// Gets the count of enemy fighter bots.
        /// </summary>
        /// <returns>The count of enemy fighter bots.</returns>
        public int GetEnemyFighterBotCount() => _gameController.GetEnemyFighterBots(GetPlayerID()).Count;

        /// <summary>
        /// Gets the count of enemy miner bots.
        /// </summary>
        /// <returns>The count of enemy miner bots.</returns>
        public int GetEnemyMinerBotCount() => _gameController.GetEnemyMinerBots(GetPlayerID()).Count;

        /// <summary>
        /// Retrieves the positions of friendly fighter bots.
        /// </summary>
        /// <returns>A list of positions of friendly fighter bots.</returns>
        public List<Pos> GetFriendlyFighterBotPositions() => Player.GetFighterBots()
                .Select(bot => bot.Pos.Clone()).ToList();

        /// <summary>
        /// Retrieves the positions of friendly miner bots.
        /// </summary>
        /// <returns>A list of positions of friendly miner bots.</returns>
        public List<Pos> GetFriendlyMinerBotPositions() => Player.GetMinerBots()
                .Select(bot => bot.Pos.Clone()).ToList();

        /// <summary>
        /// Retrieves the positions of enemy fighter bots.
        /// </summary>
        /// <returns>A list of positions of enemy fighter bots.</returns>
        public List<Pos> GetEnemyFighterBotPositions() => _gameController.GetEnemyFighterBots(GetPlayerID())
                .Select(bot => bot.Pos.Clone()).ToList();

        /// <summary>
        /// Retrieves the positions of enemy miner bots.
        /// </summary>
        /// <returns>A list of positions of enemy miner bots.</returns>
        public List<Pos> GetEnemyMinerBotPositions() => _gameController.GetEnemyMinerBots(GetPlayerID())
                .Select(bot => bot.Pos.Clone()).ToList();

        /// <summary>
        /// Gets the nearest friendly fighter bot position.
        /// </summary>
        /// <returns>The nearest friendly fighter bot position. If there are no friendly fighter bots, returns null.</returns>
        public Pos GetNearestFriendlyFighterBotPosition() => _map.fighterBSPTree.FindNearestPos(GetBotPosition(), IsFriendly());
        // public Pos GetNearestFriendlyFighterBotPosition() => GetNearestPosition(GetFriendlyFighterBotPositions());

        /// <summary>
        /// Gets the nearest friendly miner bot position.
        /// </summary>
        /// <returns>The nearest friendly miner bot position. If there are no friendly miner bots, returns null.</returns>
        public Pos GetNearestFriendlyMinerBotPosition() => _map.minerBSPTree.FindNearestPos(GetBotPosition(), IsFriendly());
        // public Pos GetNearestFriendlyMinerBotPosition() => GetNearestPosition(GetFriendlyMinerBotPositions());


        private Func<Cell, bool> IsFriendly() => c => c.gameAgent.PlayerID == GetPlayerID();
        private Func<Cell, bool> IsEnemy() => c => c.gameAgent.PlayerID != GetPlayerID();

        /// <summary>
        /// Gets the nearest enemy fighter bot position.
        /// </summary>
        /// <returns>The nearest enemy fighter bot position. If there are no enemy fighter bots, returns null.</returns>
        public Pos GetNearestEnemyFighterBotPosition() => _map.fighterBSPTree.FindNearestPos(GetBotPosition(), IsEnemy());

        // public Pos GetNearestEnemyFighterBotPosition() => GetNearestPosition(GetEnemyFighterBotPositions());

        /// <summary>
        /// Gets the nearest enemy miner bot position.
        /// </summary>
        /// <returns>The nearest enemy miner bot position. If there are no enemy miner bots, returns null.</returns>
        public Pos GetNearestEnemyMinerBotPosition() => _map.minerBSPTree.FindNearestPos(GetBotPosition(), IsEnemy());
        // public Pos GetNearestEnemyMinerBotPosition() => GetNearestPosition(GetEnemyMinerBotPositions());

        // private List<Pos> GetPositionsInShootingRange(List<Pos> botList) => botList.Where(pos => _gameAgent.Pos.InShootingRangeOf(pos))
        //         .Select(pos => pos.Clone()).ToList();

        public List<Pos> GetEnemyFighterBotsInShootingRange() => _map.fighterBSPTree.FindPosInShootingRange(GetBotPosition(), IsEnemy());

        /// <summary>
        /// Gets the positions of enemy miner bots in shooting range.
        /// </summary>
        /// <returns>A list of positions of enemy miner bots in shooting range.</returns>
        public List<Pos> GetEnemyMinerBotsInShootingRange() => _map.minerBSPTree.FindPosInShootingRange(GetBotPosition(), IsEnemy());

        /// <summary>
        /// Gets the positions of enemy fighter bots in shooting range.
        /// </summary>
        /// <returns>A list of positions of enemy fighter bots in shooting range.</returns>
        public Pos GetFriendlyMotherShipPosition() => _gameController.GetFriendlyMotherShip(GetPlayerID()).Pos.Clone();

        /// <summary>
        /// Gets the positions of enemy motherships.
        /// </summary>
        /// <returns>A list of positions of enemy motherships.</returns>
        public List<Pos> GetEnemyMotherShipPositions() => _gameController.GetEnemyMotherShips(GetPlayerID())
                .Select(enemyMotherShip => enemyMotherShip.Pos.Clone()).ToList();


        /// <summary>
        /// Gets the nearest position from a list of positions to the bot.
        /// </summary>
        /// <param name="posList">The list of positions to check.</param>
        /// <returns>The nearest position to the bot. If the list is empty, returns null.</returns>
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

        /// <summary>
        /// Gets the nearest position of an enemy mothership.
        /// </summary>
        /// <returns>The nearest position of an enemy mothership. If there are no enemy motherships, returns null.</returns>
        public Pos GetNearestEnemyMotherShipPosition() => GetNearestPosition(GetEnemyMotherShipPositions());

        /// <summary>
        /// Gets the positions of all minerals.
        /// </summary>
        /// <returns>A list of positions of all minerals.</returns>
        public List<Pos> GetMineralPositions() => _map.GetMineralPositions().Select(pos => pos.Clone()).ToList();

        /// <summary>
        /// Gets the positions of all deposits.
        /// </summary>
        /// <returns>A list of positions of all deposits.</returns>
        public List<Pos> GetDepositPositions() => _map.GetDepositPositions().Select(pos => pos.Clone()).ToList();

        /// <summary>
        /// Gets the nearest mineral's position.
        /// </summary>
        /// <returns>The nearest position of a mineral. If there are no minerals, returns null.</returns>
        public Pos GetNearestMineralPosition() => _map.mineralBSPTree.FindNearestPos(GetBotPosition());

        /// <summary>
        /// Gets the nearest deposit's position.
        /// </summary>
        /// <returns>The nearest position of a deposit. If there are no deposits, returns null.</returns>
        public Pos GetNearestDepositPosition() => _map.depositBSPTree.FindNearestPos(GetBotPosition());

        /// <summary>
        /// Gets the width of the map.
        /// </summary>
        /// <returns>The width of the map.</returns>
        public int GetMapWidth() => _map.Width;

        /// <summary>
        /// Gets the height of the map.
        /// </summary>
        /// <returns>The height of the map.</returns>
        public int GetMapHeight() => _map.Width;

        /// <summary>
        /// Determines whether a path can be found to the specified position.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>true if a path can be found to the position; otherwise, false.</returns>
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