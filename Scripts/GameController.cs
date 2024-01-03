using System;
using System.Collections.Generic;
using System.Linq;
using AI;
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
    public Godot.Collections.Array<AIAgentType> playerAgents;

    private int _currentPlayerIndex;
    private float _nextUpdateTime = -1;
    public bool GameOver;

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
            IHiveAI playerAgent = AIAgentTypeToHiveAI(playerAgents[i]);

            Vector2 startPos = playerStartPositions[i];

            if (startPos.X < 0)
                startPos.X += mapWidth - 1;
            if (startPos.Y < 0)
                startPos.Y += mapHeight - 1;

            Players.Add(new Player(i, playerAgent, new Pos((int)startPos.X, (int)startPos.Y), startMineralAmount));
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

        // check if the game is over only one player is left
        int playersRemaining = Players.Count(p => !p.HasLost);
        if (playersRemaining == 1)
        {
            GameOver = true;
            GD.Print($"Player {Players.First().PlayerID} has Won the game!!!");
            return;
        }

        foreach (Player player in Players)
        {
            player.TakeTurn(this);
        }
    }

    public IHiveAI AIAgentTypeToHiveAI(AIAgentType aiAgentType)
    {
        return aiAgentType switch
        {
            AIAgentType.Demo => new DemoAgent(),
            AIAgentType.MasterMind => new MasterMind(),
            AIAgentType.TestBot => new MinerBot(),
            AIAgentType.MinersOnly => new MinersOnly(),
            AIAgentType.EmptyBot => new EmptyBot(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public GameAgent GetGameAgentAt(Pos pos)
    {
        foreach (Player player in Players)
        {
            if (player.HasLost)
                continue;

            if (player.MotherShip.Pos.Equals(pos))
                return player.MotherShip;

            foreach (Bot fighterBot in player.GetFighterBots())
                if (fighterBot.Pos.Equals(pos))
                    return fighterBot;

            foreach (Bot minerBot in player.GetMinerBots())
                if (minerBot.Pos.Equals(pos))
                    return minerBot;
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