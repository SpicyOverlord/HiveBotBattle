using System;
using System.Collections.Generic;
using System.Linq;
using HiveMind;
using Godot;
using HiveBotBattle.Scripts;
using Utils;

public partial class GameController : Node
{
    [Export]
    public int frameRate = 60;
    [Export]
    public float updateInterval;

    [Export]
    public bool debugTurn;
    [ExportGroup("Level Parameters")]

    [Export]
    public float mapScale = 1;
    [Export]
    public int mapWidth = 50;
    [Export]
    public int mapHeight = 25;
    [Export]
    public int startMineralAmount = 1;
    [Export]
    public Godot.Collections.Array<Vector2> playerStartPositions;
    [Export]
    public Godot.Collections.Array<HiveMindType> playerHiveMinds;

    private int _currentPlayerIndex;
    private float _nextUpdateTime = -1;
    public bool GameOver => Players.Count(p => !p.HasLost) == 1;

    public Map Map;

    public List<Player> Players;

    public int TurnCount = 1;

    private Vector2 lastScreenSize;
    public override void _Ready()
    {
        base._Ready();

        Engine.MaxFps = frameRate;
        Map = GetParent().GetNode<Map>("Map");
        lastScreenSize = new Vector2(0, 0);

        SetupPlayers();
        // create map terrain with players' motherships
        Map.CreateMap(mapWidth, mapHeight, Players);
    }

    private void SetupPlayers()
    {
        Players = new List<Player>();
        for (int i = 0; i < playerStartPositions.Count; i++)
        {
            IHiveMind playerHiveMind = HiveMindTypeToHiveMind(playerHiveMinds[i]);

            Vector2 startPos = playerStartPositions[i];

            if (startPos.X < 0)
                startPos.X += mapWidth - 1;
            if (startPos.Y < 0)
                startPos.Y += mapHeight - 1;

            Players.Add(new Player(i, playerHiveMind, new Pos((int)startPos.X, (int)startPos.Y), startMineralAmount));
        }
    }

    private readonly Queue<double> frameRates = new Queue<double>();
    private const int MaxFrameRateSamples = 1000;
    private int frameCount = 0;

    public override void _Process(double delta)
    {
        if (!debugTurn)
        {
            frameRates.Enqueue(Engine.GetFramesPerSecond());

            if (frameRates.Count > MaxFrameRateSamples)
                frameRates.Dequeue();

            frameCount++;
            if (frameCount % MaxFrameRateSamples == 0)
                GD.Print($"Average frame-rate last {MaxFrameRateSamples} frames: {frameRates.Average()}");
        }

        // set map position and scale to center of level
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        if (lastScreenSize != screenSize)
        {
            lastScreenSize = screenSize;
            Map.PositionAndScale(mapScale, mapWidth, mapHeight, screenSize);
        }

        if (GameOver) return;

        if (debugTurn && !Input.IsActionJustPressed("debug")) return;

        if (!debugTurn)
        {
            if (Time.GetTicksMsec() >= _nextUpdateTime)
                _nextUpdateTime = Time.GetTicksMsec() + updateInterval * 1000;
            else
                return;
        }

        foreach (Player player in Players)
        {
            player.TakeTurn(this);
        }

        // check if the game is over only one player is left
        if (GameOver)
        {
            GD.Print($"Player {Players.First().PlayerID} has Won the game!!!");
            return;
        }
    }

    public static IHiveMind HiveMindTypeToHiveMind(HiveMindType HiveMindType)
    {
        return HiveMindType switch
        {
            HiveMindType.Demo => new DemoHiveMind(),
            HiveMindType.MasterMind => new MasterMind(),
            HiveMindType.TestBot => new MinerBot(),
            HiveMindType.MinersOnly => new MinersOnly(),
            HiveMindType.EmptyBot => new EmptyBot(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public GameAgent GetGameAgentAt(Pos pos)
    {
        Map.Cell fighterBot = Map.GetFighterBotCell(pos);
        if (fighterBot is not null)
            return fighterBot.gameAgent;

        Map.Cell minerBot = Map.GetMinerBotCell(pos);
        if (minerBot is not null)
            return minerBot.gameAgent;

        foreach (Player player in Players)
        {
            if (player.MotherShip.Pos == pos)
                return player.MotherShip;
        }

        return null;
    }

    public MotherShip GetFriendlyMotherShip(int playerID) => Players.Where(p => p.PlayerID == playerID).FirstOrDefault().MotherShip;

    public List<MotherShip> GetEnemyMotherShips(int playerID) => Players.Where(p => !p.HasLost && playerID != p.PlayerID).Select(p => p.MotherShip).ToList();

    public List<Bot> GetEnemyFighterBots(int playerID)
    {
        List<Bot> allEnemyFighterBots = new List<Bot>();
        foreach (Player player in Players.Where(p => !p.HasLost && playerID != p.PlayerID))
            allEnemyFighterBots.AddRange(player.GetFighterBots());

        return allEnemyFighterBots;
    }

    public List<Bot> GetEnemyMinerBots(int playerID)
    {
        List<Bot> allEnemyMinerBots = new List<Bot>();
        foreach (Player player in Players.Where(p => !p.HasLost && playerID != p.PlayerID))
            allEnemyMinerBots.AddRange(player.GetMinerBots());

        return allEnemyMinerBots;
    }
}