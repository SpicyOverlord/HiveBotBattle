using System.Linq;

namespace HiveBotBattle.Scripts.BSPTree
{
    /// <summary>
    /// A resizing array of Map Cells.
    /// </summary>
    public class ResizingCellArray
    {
        /// <summary>
        /// The percentage of destroyed cells required to trigger a clean operation.
        /// </summary>
        public const float DestroyedCleanPercentage = 0.2f;

        private int _lastIndex;
        private Map.Cell[] _cellArray;
        private int _destroyedCellCount;

        /// <summary>
        /// Gets the number of cells in the array.
        /// </summary>
        public int Count => _lastIndex;

        /// <summary>
        /// Checks if the array is empty.
        /// </summary>
        /// <returns>True if the array is empty, otherwise false.</returns>
        public bool IsEmpty() => Count == 0;

        /// <summary>
        /// Initializes a new instance of the ResizingCellArray class with the specified start size.
        /// </summary>
        /// <param name="startSize">The initial size of the array. Default is 16.</param>
        public ResizingCellArray(int startSize = 16)
        {
            _destroyedCellCount = 0;
            _lastIndex = 0;
            _cellArray = new Map.Cell[startSize];
        }

        /// <summary>
        /// Gets the cell at the specified index.
        /// </summary>
        /// <param name="index">The index of the cell to retrieve.</param>
        /// <returns>The cell at the specified index.</returns>
        public Map.Cell Get(int index) => _cellArray[index];

        /// <summary>
        /// Adds a cell to the array.
        /// If the array is full, the array size is doubled.
        /// </summary>
        /// <param name="cell">The cell to add.</param>
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

        /// <summary>
        /// Increments the count of destroyed cells.
        /// </summary>
        public void IncrementDestroyedCells()
        {
            _destroyedCellCount++;
        }

        /// <summary>
        /// Gets the count of destroyed cells.
        /// </summary>
        /// <returns>The count of destroyed cells.</returns>
        public int GetDestroyedCellCount()
        {
            return _destroyedCellCount;
        }

        /// <summary>
        /// Cleans the array if the percentage of destroyed cells exceeds the threshold.
        /// </summary>
        public void CleanIfNeeded()
        {
            // do nothing if the destroyed cell count is less than the threshold
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

            _cellArray = newArray;
            _destroyedCellCount = 0;
        }

        /// <summary>
        /// Converts the array to a standard array, excluding null cells and destroyed cells.
        /// </summary>
        /// <returns>An array of Map.Cells.</returns>
        public Map.Cell[] ToStandardArray() => _cellArray.Where(c => c is not null && !c.IsDestroyed).ToArray();
    }
}