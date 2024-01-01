using System.Diagnostics;
using System.Linq;

namespace HiveBotBattle.Scripts.BSPTree
{
    public class ResizingCellArray
    {
        public const float DestroyedCleanPercentage = 0.2f;
        private int _lastIndex;
        private Map.Cell[] _cellArray;
        public int Count => _lastIndex;
        public bool IsEmpty() => Count == 0;

        private int _destroyedCellCount;

        public ResizingCellArray(int startSize = 16)
        {
            _destroyedCellCount = 0;
            _lastIndex = 0;
            _cellArray = new Map.Cell[startSize];
        }

        public Map.Cell Get(int index) => _cellArray[index];

        public void Add(Map.Cell cell)
        {
            if (Count >= _cellArray.Length)
                DoubleArrayLength();

            _cellArray[_lastIndex] = cell;
            _lastIndex++;
        }

        private void DoubleArrayLength()
        {
            Map.Cell[] newArray = new Map.Cell[_cellArray.Length * 2];
            for (int i = 0; i < _cellArray.Length; i++)
                newArray[i] = _cellArray[i];
            
            _cellArray = newArray;
        }

        public void IncrementDestroyedCells()
        {
            _destroyedCellCount++;
        }

        public int GetDestroyedCellCount()
        {
            return _destroyedCellCount;
        }

        public void CleanIfNeeded()
        {
            if (GetDestroyedCellCount() < _cellArray.Length * DestroyedCleanPercentage)
                return;

            Map.Cell[] newArray = new Map.Cell[_cellArray.Length];

            int deletedCells = 0;
            for (int i = 0; i < Count; i++)
            {
                Map.Cell cell = _cellArray[i];
                if (cell.IsDestroyed)
                    deletedCells++;
                else
                    newArray[i - deletedCells] = cell;
            }

            _lastIndex -= deletedCells;

            // GD.Print("CLEANED! Removed " + deletedCells + " destroyed cells! from " + _cellArray.Length);

            _cellArray = newArray;
            _destroyedCellCount = 0;
        }

        public Map.Cell[] ToStandardArray() => _cellArray.Where(c => c is not null && !c.IsDestroyed).ToArray();
    }
}