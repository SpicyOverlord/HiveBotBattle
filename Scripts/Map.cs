using Godot;
using HiveBotBattle.Scripts;
using HiveBotBattle.Scripts.BSPTree;
using HiveBotBattle.Scripts.Utils.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

public partial class Map : Node2D
{
    #region sprites
    [ExportGroup("Prefabs")]
    [Export]
    public PackedScene emptySpritePrefab;
    [Export]
    public PackedScene explosionPrefab;
    [Export]
    public PackedScene minedPrefab;

    [ExportGroup("Bot Textures")]
    [Export]
    public Texture2D stoneSprite;
    [Export]
    public Texture2D depositSprite;
    [Export]
    public Texture2D mineralSprite;
    [Export]
    public Texture2D bedrockSprite;

    [Export]
    public Texture2D fighterBotRedSprite;
    [Export]
    public Texture2D fighterBotGreenSprite;
    [Export]
    public Texture2D fighterBotPurpleSprite;

    [Export]
    public Texture2D fighterBotCyanSprite;

    [Export]
    public Texture2D minerBotRedSprite;
    [Export]
    public Texture2D minerBotGreenSprite;
    [Export]
    public Texture2D minerBotPurpleSprite;

    [Export]
    public Texture2D minerBotCyanSprite;

    [Export]
    public Texture2D motherShipRedSprite;
    [Export]
    public Texture2D motherShipGreenSprite;
    [Export]
    public Texture2D motherShipPurpleSprite;

    [Export]
    public Texture2D motherShipCyanSprite;

    #endregion

    private Cell[,] _fighterBotMap;
    private Cell[,] _minerBotMap;
    private Cell[,] _map;

    public BSPTree mineralBSPTree;
    public BSPTree depositBSPTree;
    public BSPTree fighterBSPTree;
    public BSPTree minerBSPTree;

    public int Height;
    public int Width;


    public void CreateMap(int width, int height, List<Player> players)
    {
        InitializeMapProperties(width, height);
        CreateLevelLayout(width, height, players);
        AddMotherShipVisuals(players);
    }

    private void InitializeMapProperties(int width, int height)
    {
        Width = width;
        Height = height;

        InitializeQuadTrees(width, height);
        InitializeCellArrays(width, height);
    }

    private void InitializeQuadTrees(int width, int height)
    {
        int cellCount = width * height;
        int BSPTreeDepth = (int)Math.Floor(Math.Log2(cellCount / 100));

        mineralBSPTree = new BSPTree(width, height, BSPTreeDepth);
        depositBSPTree = new BSPTree(width, height, BSPTreeDepth);
        minerBSPTree = new BSPTree(width, height, BSPTreeDepth);
        fighterBSPTree = new BSPTree(width, height, BSPTreeDepth);
    }

    private void InitializeCellArrays(int width, int height)
    {
        _minerBotMap = new Cell[width, height];
        _fighterBotMap = new Cell[width, height];
        _map = new Cell[width, height];
    }

    private void AddMotherShipVisuals(List<Player> players)
    {
        foreach (Player player in players)
            CreateCell(player.MotherShip.Pos, CellType.MotherShip, player.PlayerID, player.MotherShip);
    }

    private void CreateLevelLayout(int width, int height, List<Player> players)
    {
        // create level layout
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                // bedrock edges
                if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                {
                    CreateCell(x, y, CellType.Bedrock);
                    continue;
                }
                if (y == 1 || y == height - 2 || x == 1 || x == width - 2)
                {
                    CreateCell(x, y, CellType.Stone);
                    continue;
                }

                // skip, if cell is next to or on a players MotherShip
                if (players.Any(player => player.MotherShip.Pos.IsNextToOrEqual(x, y) || player.MotherShip.Pos.Equals(x, y)))
                    continue;

                // create cell, if it's value is not none or empty
                CellType cellType = LvlGenerator.DiagonalLines(x, y);
                if (cellType != CellType.None && cellType != CellType.Empty)
                    CreateCell(x, y, cellType);

            }
    }

    public void PositionAndScale(float mapScale, int mapWidth, int mapHeight, Vector2 screenSize)
    {
        // makes scale == 1 be the normal fit
        mapScale *= 37;

        float xRatio = screenSize.Y / screenSize.X;
        float yRatio = screenSize.X / screenSize.Y;
        mapScale /= Mathf.Max(mapWidth, mapHeight) * Mathf.Max(xRatio, yRatio);
        Scale = new Vector2(mapScale, mapScale);

        float xPos = (screenSize.X - mapWidth * Pos.PositionSpacing * mapScale) / 2f;
        float yPos = (screenSize.Y - mapHeight * Pos.PositionSpacing * mapScale) / 2f;
        Position = new Vector2(xPos, yPos);
    }

    public Cell CreateCell(int x, int y, CellType cellType, int playerID = -1, GameAgent gameAgent = null) => CreateCell(new Pos(x, y), cellType, playerID, gameAgent);
    public Cell CreateCell(Pos pos, CellType cellType, int playerID = -1, GameAgent gameAgent = null)
    {
        if (cellType is CellType.None or CellType.Empty)
            throw new Exception("Can't create visual of type: " + cellType.ToString());
        if (!IsEmpty(pos))
            throw new Exception("Pos already filled with visual: " + pos);

        bool isCreatingBot = cellType is CellType.FighterBot or CellType.MinerBot;

        if (isCreatingBot && gameAgent is null)
            throw new Exception("Bot is null, but cellType is: " + cellType.ToString());

        Node2D spriteTransform = CreateSprite(pos, cellType, playerID);
        Cell cell = new Cell(pos, spriteTransform, cellType, gameAgent);

        if (isCreatingBot)
        {
            if (cellType is CellType.MinerBot)
                _minerBotMap[pos.X, pos.Y] = cell;
            else if (cellType is CellType.FighterBot)
                _fighterBotMap[pos.X, pos.Y] = cell;
        }
        else
            _map[pos.X, pos.Y] = cell;

        // add cell to corosponsing BSPTree
        if (cellType == CellType.Deposit)
            depositBSPTree.Insert(cell);
        else if (cellType == CellType.Mineral)
            mineralBSPTree.Insert(cell);
        else if (cellType == CellType.FighterBot)
            fighterBSPTree.Insert(cell);
        else if (cellType == CellType.MinerBot)
            minerBSPTree.Insert(cell);

        return cell;
    }
    public Node2D CreateSprite(Pos pos, CellType cellType, int playerID = -1)
    {
        Sprite2D spriteObject = emptySpritePrefab.Instantiate() as Sprite2D;
        spriteObject.Position = new Vector2(pos.X, pos.Y);

        Texture2D sprite = CellTypeToSprite(cellType, playerID);
        spriteObject.Texture = sprite;

        AddChild(spriteObject);

        return spriteObject;
    }
    private Texture2D CellTypeToSprite(CellType cellType, int playerID = -1)
    {
        if (playerID == -1)
            return cellType switch
            {
                CellType.None => null,
                CellType.Empty => null,
                CellType.Stone => stoneSprite,
                CellType.Deposit => depositSprite,
                CellType.Mineral => mineralSprite,
                CellType.Bedrock => bedrockSprite,
                // CellType.FighterBot => fighterBotPrefab,
                // CellType.MinerBot => minerBotPrefab,
                // CellType.MotherShip => motherShipPrefab,
                _ => throw new Exception("(" + playerID + ") CellType '" + cellType.ToString() +
                                         "' not recognized!")
            };

        if (playerID == 0)
            return cellType switch
            {
                CellType.FighterBot => fighterBotRedSprite,
                CellType.MinerBot => minerBotRedSprite,
                CellType.MotherShip => motherShipRedSprite,
                _ => throw new Exception("(" + playerID + ") CellType '" + cellType.ToString() +
                                         "' not recognized!")
            };

        if (playerID == 1)
            return cellType switch
            {
                CellType.FighterBot => fighterBotPurpleSprite,
                CellType.MinerBot => minerBotPurpleSprite,
                CellType.MotherShip => motherShipPurpleSprite,
                _ => throw new Exception("(" + playerID + ") CellType '" + cellType.ToString() +
                                         "' not recognized!")
            };

        if (playerID == 2)
            return cellType switch
            {
                CellType.FighterBot => fighterBotGreenSprite,
                CellType.MinerBot => minerBotGreenSprite,
                CellType.MotherShip => motherShipGreenSprite,
                _ => throw new Exception("(" + playerID + ") CellType '" + cellType.ToString() +
                                         "' not recognized!")
            };

        if (playerID == 3)
            return cellType switch
            {
                CellType.FighterBot => fighterBotCyanSprite,
                CellType.MinerBot => minerBotCyanSprite,
                CellType.MotherShip => motherShipCyanSprite,
                _ => throw new Exception("(" + playerID + ") CellType '" + cellType.ToString() +
                                         "' not recognized!")
            };

        throw new Exception("Player ID '" + playerID + "' not recognized");
    }

    public void DestroyVisual(Pos pos)
    {
        if (_map[pos.X, pos.Y] is not null)
        {
            Cell cellToBeDestroyed = _map[pos.X, pos.Y];
            _map[pos.X, pos.Y] = null;

            cellToBeDestroyed.Destroy();

            // tell BSPTree that a cell has been destroyed
            switch (cellToBeDestroyed.CellType)
            {
                case CellType.Deposit:
                    depositBSPTree.IncrementDestroyedCells();
                    depositBSPTree.ReinsertAllAndCleanIfNeeded();
                    break;
                case CellType.Mineral:
                    mineralBSPTree.IncrementDestroyedCells();
                    mineralBSPTree.ReinsertAllAndCleanIfNeeded();
                    break;
            }

            return;
        }

        bool isFighterBot = _fighterBotMap[pos.X, pos.Y] is not null;
        bool isMinerBot = _minerBotMap[pos.X, pos.Y] is not null;

        if (isFighterBot || isMinerBot)
        {
            Node2D explotionEffect = explosionPrefab.Instantiate() as Node2D;
            explotionEffect.Position = pos.GetAsVector2();
            AddChild(explotionEffect);

            // only destroy bot if it should be destroyed
            if (isMinerBot && _minerBotMap[pos.X, pos.Y].gameAgent.ShouldBeDestroyed)
            {
                _minerBotMap[pos.X, pos.Y].Destroy();
                _minerBotMap[pos.X, pos.Y] = null;
            }
            else if (isFighterBot && _fighterBotMap[pos.X, pos.Y].gameAgent.ShouldBeDestroyed)
            {
                _fighterBotMap[pos.X, pos.Y].Destroy();
                _fighterBotMap[pos.X, pos.Y] = null;
            }
            return;
        }

        throw new Exception("Cell at " + pos + " is not filled!");
    }

    public Cell GetCell(int x, int y) => _map[x, y];
    public Cell GetCell(Pos pos) => GetCell(pos.X, pos.Y);

    public void MoveBotTo(Bot bot, Pos moveTarget)
    {
        Cell[,] botMap = bot.Type == AgentType.MinerBot ? _minerBotMap : _fighterBotMap;
        Cell botCell = botMap[bot.Pos.X, bot.Pos.Y];
        Cell moveTargetCell = botMap[moveTarget.X, moveTarget.Y];

        if (botCell is null)
            throw new Exception("No bot found at bot position: " + bot.Pos);
        if (moveTargetCell is not null)
            throw new Exception("Move target " + moveTarget + " is not empty! " +
                                moveTargetCell.CellType.ToString());
        if (!bot.Pos.IsNextToOrEqual(moveTarget))
            throw new Exception("Move target " + moveTarget + " is not next to bot " + bot.Pos);

        if (bot.Type == AgentType.MinerBot)
            MoveMinerBotCell(botCell.GetPosition(), moveTarget);
        else if (bot.Type == AgentType.FighterBot)
            MoveFighterBotCell(botCell.GetPosition(), moveTarget);

        // botMap[bot.Pos.X, bot.Pos.Y] = null;
        // botMap[moveTarget.X, moveTarget.Y] = botCell;
        // botCell.MoveTo(moveTarget);
    }
    private void MoveMinerBotCell(Pos fromPos, Pos toPos)
    {
        Cell cell = _minerBotMap[fromPos.X, fromPos.Y];
        _minerBotMap[fromPos.X, fromPos.Y] = null;
        _minerBotMap[toPos.X, toPos.Y] = cell;

        cell.MoveTo(toPos);
    }

    private void MoveFighterBotCell(Pos fromPos, Pos toPos)
    {
        Cell cell = _fighterBotMap[fromPos.X, fromPos.Y];
        _fighterBotMap[fromPos.X, fromPos.Y] = null;
        _fighterBotMap[toPos.X, toPos.Y] = cell;

        cell.MoveTo(toPos);
    }

    public Cell GetMinerBotCell(int x, int y) => _minerBotMap[x, y];
    public Cell GetMinerBotCell(Pos pos) => GetMinerBotCell(pos.X, pos.Y);
    public Cell GetFighterBotCell(int x, int y) => _fighterBotMap[x, y];
    public Cell GetFighterBotCell(Pos pos) => GetFighterBotCell(pos.X, pos.Y);
    public List<Pos> GetMineralPositions() => mineralBSPTree.GetPosList();
    public List<Pos> GetDepositPositions() => depositBSPTree.GetPosList();
    public void Mine(Pos mineTarget)
    {
        CellType cellType = GetCell(mineTarget).CellType;

        if (cellType is not (CellType.Stone or CellType.Deposit))
            throw new Exception("Can't mine target " + mineTarget + ", as it is " +
                                cellType.ToString());

        DestroyVisual(mineTarget);

        Node2D minedEffect = minedPrefab.Instantiate() as Node2D;
        minedEffect.Position = mineTarget.GetAsVector2();
        AddChild(minedEffect);

        if (cellType is CellType.Deposit)
            CreateCell(mineTarget, CellType.Mineral);
    }
    public void ClearMineral(Pos pos)
    {
        if (!IsMineral(pos))
            throw new Exception("No mineral in cell: " + pos);

        DestroyVisual(pos);
    }
    public Pos GetFirstEmptyNeighbor(Pos pos)
    {
        // loop over neighboring cells
        for (int yDiff = 1; yDiff >= -1; yDiff--)
            for (int xDiff = -1; xDiff <= 1; xDiff++)
            {
                if (xDiff == 0 && yDiff == 0) continue;

                // return cell position if it is empty
                if (IsEmpty(pos.X + xDiff, pos.Y + yDiff))
                    return new Pos(pos.X + xDiff, pos.Y + yDiff);
            }

        // return null if no empty neighbors
        return null;
    }

    public bool IsCellType(Pos pos, CellType cellType)
    {
        Cell cell;
        if (cellType is CellType.MinerBot)
            cell = GetMinerBotCell(pos);
        else if (cellType is CellType.FighterBot)
            cell = GetFighterBotCell(pos);
        else
            cell = GetCell(pos);

        if (cell is null)
            return cellType is CellType.Empty or CellType.None;

        return cell.CellType == cellType;
    }

    public bool IsStone(Pos pos) => GetCell(pos)?.CellType == CellType.Stone;
    public bool IsDeposit(Pos pos) => GetCell(pos)?.CellType == CellType.Deposit;
    public bool IsMineral(Pos pos) => GetCell(pos)?.CellType == CellType.Mineral;
    public bool IsBedrock(Pos pos) => GetCell(pos)?.CellType == CellType.Bedrock;
    public bool IsMotherShip(Pos pos) => GetCell(pos)?.CellType == CellType.MotherShip;
    public bool IsMinerBot(Pos pos) => IsMinerBot(pos.X, pos.Y);
    public bool IsMinerBot(int x, int y) => GetMinerBotCell(x, y) is not null;
    public bool IsFighterBot(Pos pos) => IsFighterBot(pos.X, pos.Y);
    public bool IsFighterBot(int x, int y) => GetFighterBotCell(x, y) is not null;

    public bool IsShootable(Pos pos) => GetMinerBotCell(pos) is not null || GetFighterBotCell(pos) is not null || GetCell(pos)?.CellType is CellType.MotherShip;
    public bool IsMineable(Pos pos) => IsMineable(pos.X, pos.Y);
    public bool IsMineable(int x, int y) => GetCell(x, y)?.CellType is CellType.Stone or CellType.Deposit;

    public bool IsEmpty(int x, int y) => GetCell(x, y) is null && GetFighterBotCell(x, y) is null && GetMinerBotCell(x, y) is null;
    public bool IsEmpty(Pos pos) => IsEmpty(pos.X, pos.Y);
    public bool IsWalkable(Pos pos) => IsWalkable(pos.X, pos.Y);
    public bool IsWalkable(int x, int y) => GetCell(x, y) is null || GetCell(x, y)?.CellType is CellType.Mineral;
    public bool IsBlocked(Pos pos, bool canMine = false) => IsBlocked(pos.X, pos.Y, canMine);
    public bool IsBlocked(int x, int y, bool canMine = false) => canMine ? !IsWalkable(x, y) && !IsMineable(x, y) : !IsWalkable(x, y);
    public bool IsSurrounded(Pos pos, bool canMine = false) => IsSurrounded(pos.X, pos.Y, canMine);
    public bool IsSurrounded(int x, int y, bool canMine = false)
    {
        for (int yDiff = -1; yDiff <= 1; yDiff++)
            for (int xDiff = -1; xDiff <= 1; xDiff++)
            {
                if (xDiff == 0 && yDiff == 0) continue;

                if (!IsBlocked(x + xDiff, y + yDiff, canMine))
                {
                    return false;
                }
            }


        return true;
    }
}