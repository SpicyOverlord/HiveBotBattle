using Godot;
using HiveBotBattle.Scripts.Utils.Types;
using Utils;

public partial class Map
{
    public class Cell
    {
        public readonly CellType CellType;
        private readonly Node2D CellNode;
        private Pos _pos;

        private bool _isDestroyed;

        public Cell(Pos pos, Node2D node, CellType cellType)
        {
            CellNode = node;
            CellNode.Position = pos.GetAsVector2();
            _pos = pos.Clone();

            CellType = cellType;

            _isDestroyed = false;
        }
        
        public bool IsDestroyed => _isDestroyed;
        public Pos GetPosition() => _pos;

        public void MoveTo(Pos pos)
        {
            CellNode.Position = pos.GetAsVector2();
            _pos = pos.Clone();
        }

        public void Destroy()
        {
            CellNode.QueueFree();            
            _isDestroyed = true;
        }
    }
}