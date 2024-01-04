# HiveBotBattle

HiveBotBattle is a turn-based PvP (Player vs. Player) coding game where players program a "HiveMind" to play the game on their behalf. To win, your HiveMind must effectively manage resources and strategically fight against the other players HiveMinds and destroy their MotherShips. 

This game is inspired by PokerBotBattle.

## How to Play

In HiveBotBattle, you are required to create 3 types of Agent "AIs":

1. **FighterBot**:
   - The FighterBot can move and shoot other bots. Its primary role is to combat opposing players' bots and destroy their MotherShips.

2. **MinerBot**:
   - The MinerBot is capable of moving, mining deposits, and collecting Minerals. Its main task is to transport Minerals back to the MotherShip, which uses these Minerals to manufacture new bots.
   - A MinerBot can carry a maximum of 3 Minerals and delivers all its Minerals to the MotherShip in a single turn.

3. **MotherShip**:
   - The MotherShip is a crucial component. It is immobile, and its destruction means the loss of the game, with all your bots being destroyed instantly.
   - The MotherShip constructs Fighter and Miner bots using Minerals collected by MinerBots.

Each Agent must implement a function in the IHiveAI interface, which takes an Observation as input and returns a Move.

- An **Observation** is an object containing all the information known to the Agent, along with a list of helper functions for acquiring additional details.
- A **Move** is an object that the function must return, containing the necessary details for executing the action in the game.

Each turn, the game calls the corresponding function for each active Agent.

The game is won when a player successfully destroys all opposing Motherships.

## Important Information

- When a player's MotherShip is destroyed, they lose the game, and all their bots are destroyed immediately.
- The cost of building new bots increases proportionally to the number of bots your HiveMind already controls.
  - Cost of Miners: `(MinerBotCount * MinerBotCount * 0.2) + 1`
  - Cost of Fighters: `((FighterBotCount + 2) * FighterBotCount * 0.2) + 1`

---

## HiveBotBattle: Terms and Definitions

**HiveMind**: The central AI program created by the player to control all actions of their bots in the game.

**Agent "AIs"**: The individual AI units controlled by the HiveMind, each with specific roles and capabilities.

**FighterBot**: A type of Agent AI designed for combat. Its primary functions include moving and shooting enemy bots and destroying Motherships.

**MinerBot**: An Agent AI specialized in resource gathering. It mines deposits to collect Minerals and transports them to the MotherShip.

**MotherShip**: A crucial, non-mobile Agent AI. It manufactures new bots and its destruction leads to the player's loss.

**Minerals**: The primary resource in the game, collected by MinerBots and used by the MotherShip to create new bots.

**Deposits**: Locations on the game map where Minerals can be mined by MinerBots.

**IHiveAI Interface**: The programming interface that players use to implement the decision-making logic for their HiveMind and bots.

**Observation**: An object containing all information accessible to an Agent at a given turn, including environment data and helper functions.

**Move**: The action output from an Agent's function, dictating its behavior in a given turn.

**Game Turn**: A single cycle of play in which all Agents perform actions based on their programmed instructions.

**Game Map**: The virtual environment in which the game takes place, containing various elements like deposits and locations for Agents.

**Bot Manufacturing**: The process by which the MotherShip uses collected Minerals to create new FighterBots and MinerBots.

**Resource Management**: The strategic handling of Minerals, including their collection, transportation, and use in bot production.

**Combat Mechanics**: The rules and actions governing how FighterBots engage with enemy bots and Motherships.

**Mining Mechanics**: The system that defines how MinerBots extract Minerals from deposits.

**Capacity (MinerBot)**: The maximum number of Minerals a MinerBot can carry at one time.

**Bot Cost Formula**: The mathematical formula used to calculate the Mineral cost for creating new bots, varying based on the number of existing bots.
