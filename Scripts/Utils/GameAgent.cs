using HiveBotBattle.Scripts.Utils.Types;

namespace Utils
{
    public abstract class GameAgent
    {
        public readonly int PlayerID;
        public Pos Pos { get; protected set; }
        public readonly BotType Type;

        public bool IsDestroyed { get; protected set; }
        public int Health { get; protected set; }

        protected GameAgent(int playerID, BotType botType, Pos startPos, int startHealth)
        {
            PlayerID = playerID;
            Type = botType;
            Pos = startPos;
            Health = startHealth;
            IsDestroyed = false;
        }

        public void MoveTo(Pos pos) => Pos = pos.Clone();


        public void Damage(int damage)
        {
            if (IsDestroyed) return;

            Health -= damage;
            if (Health <= 0) IsDestroyed = true;
        }
    }
}