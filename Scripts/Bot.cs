using System;
using System.Diagnostics;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;

namespace HiveBotBattle.Scripts
{
    public class Bot : GameAgent
    {
        public const int DamageAmount = 10;

        public const float ShootingRange = 2.85f;

        public readonly int BotID;
        public const int MaxHealth = 100;
        public const int HealAmount = 5;
        public const int MaxPickedUpMinerals = 3;
        public int PickedUpMinerals { get; private set; }


        public Bot(int playerID, int botID, BotType type, Pos startPosition) : base(playerID, type, startPosition,
            MaxHealth)
        {
            BotID = botID;
            IsDestroyed = false;
        }

        public void Heal()
        {
            Health += HealAmount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        public bool CanPickUpMinerals() => PickedUpMinerals < MaxPickedUpMinerals;

        public bool AddMineral()
        {
            if (!CanPickUpMinerals()) throw new Exception("Can't pick up more Minerals");
            PickedUpMinerals++;
            return true;
        }

        public void RemovePickedUpMinerals() => PickedUpMinerals = 0;
    }
}