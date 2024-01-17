using Godot;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;

public partial class Map
{
    public class Cell
    {
        public readonly CellType CellType;
        public readonly GameAgent gameAgent;
        private readonly Node2D CellNode;
        private Pos _pos;

        private bool _shouldBeDestroyed;

        public Cell(Pos pos, Node2D node, CellType cellType, GameAgent gameAgent = null)
        {
            CellNode = node;
            CellNode.Position = pos.GetAsVector2();
            _pos = pos.Clone();

            gameAgent?.MoveTo(pos);

            this.gameAgent = gameAgent;
            CellType = cellType;

            _shouldBeDestroyed = false;
        }

        public bool IsDestroyed => _shouldBeDestroyed;
        public Pos GetPosition() => _pos;

        public void MoveTo(Pos pos)
        {
            CellNode.Position = pos.GetAsVector2();
            _pos = pos.Clone();
            gameAgent?.MoveTo(pos);
        }

        public void Destroy()
        {
            CellNode.Position = new Vector2(-1000, -1000);
            gameAgent?.Damage(99999);

            CellNode.QueueFree();
            _shouldBeDestroyed = true;
        }
    }
}