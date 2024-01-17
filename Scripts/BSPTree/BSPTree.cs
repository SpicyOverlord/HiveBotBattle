using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using Utils;
using static Map;

namespace HiveBotBattle.Scripts.BSPTree
{
    public class BSPTree
    /// <summary>
    /// Represents a Binary Space Partition (BSP) tree used for spatial partitioning of elements.
    /// </summary>
    {
        public const int Null = -1;
        public const int Empty = -2;

        private readonly Partition[] _partitions;
        private int _partitionsLastIndex;
        private readonly Partition[] _leafPartitions;
        private int _leafPartitionLastIndex;

        public readonly List<ElementIndex> ElementIndices;

        public readonly ResizingCellArray CellArray;

        /// <summary>
        /// Initializes a Binary Space Partitioning (BSP) tree used for nearest neighbor search in 2D space.
        /// </summary>
        /// <param name="width">The width of the 2D space.</param>
        /// <param name="height">The height of the 2D space.</param>
        /// <param name="TreeDepth">The depth of the BSP tree.</param>
        public BSPTree(int width, int height, int TreeDepth)
        {

            ElementIndices = new List<ElementIndex>();
            CellArray = new ResizingCellArray();
            _partitions = new Partition[CalculatePartitionCount(TreeDepth)];

            Bounds2D bounds = new Bounds2D(0, 0, width, height);

            _partitions[_partitionsLastIndex] = new Partition(bounds);
            _partitionsLastIndex++;

            CreateTreePartitions(0, TreeDepth);

            _leafPartitions = CollectLeafPartitions(TreeDepth);
        }


        /// <summary>
        /// Recursively creates partitions in the binary space partition tree.
        /// </summary>
        /// <param name="partitionIndex">The index of the current partition.</param>
        /// <param name="maxTreeDepth">The maximum depth of the tree.</param>
        private void CreateTreePartitions(int partitionIndex, int maxTreeDepth)
        {
            Partition partition = _partitions[partitionIndex];

            if (partition.PartitionDepthInTree == maxTreeDepth)
            {
                partition.FirstElementIndex = Empty;
                return;
            }

            CreatePartitionChildren(partitionIndex);

            CreateTreePartitions(partition.RightChildIndex, maxTreeDepth);
            CreateTreePartitions(partition.LeftChildIndex, maxTreeDepth);
        }
        /// <summary>
        /// Creates the 2 children partitions for a given partition in the BSP tree.
        /// </summary>
        /// <param name="partitionIndex">The index of the partition to create children for.</param>
        private void CreatePartitionChildren(int partitionIndex)
        {
            Partition partition = _partitions[partitionIndex];

            Bounds2D bounds = partition.Bounds;

            // calculate bounds of children
            Bounds2D leftChildBounds;
            Bounds2D rightChildBounds;
            if (partition.SplitAlongXAxis)
            {
                leftChildBounds = new Bounds2D(bounds.xMin, partition.SplitValue, bounds.yMin, bounds.yMax);
                rightChildBounds = new Bounds2D(partition.SplitValue, bounds.xMax, bounds.yMin, bounds.yMax);
            }
            else
            {
                leftChildBounds = new Bounds2D(bounds.xMin, bounds.xMax, bounds.yMin, partition.SplitValue);
                rightChildBounds = new Bounds2D(bounds.xMin, bounds.xMax, partition.SplitValue, bounds.yMax);
            }

            _partitions[_partitionsLastIndex] = new Partition(leftChildBounds, partition.PartitionDepthInTree + 1);
            partition.SetLeftChildIndex(_partitionsLastIndex);
            _partitionsLastIndex++;

            _partitions[_partitionsLastIndex] = new Partition(rightChildBounds, partition.PartitionDepthInTree + 1);
            partition.SetRightChildIndex(_partitionsLastIndex);
            _partitionsLastIndex++;
        }

        /// <summary>
        /// Calculates the total number of partitions in the BSP tree based on the maximum tree depth.
        /// </summary>
        /// <param name="maxTreeDepth">The maximum depth of the tree.</param>
        /// <returns>The total number of partitions in the BSP tree.</returns>
        public static int CalculatePartitionCount(int maxTreeDepth) => (int)Mathf.Pow(2, maxTreeDepth + 1) - 1;

        /// <summary>
        /// Calculates the total number of leaf partitions in the BSP tree based on the maximum tree depth.
        /// </summary>
        /// <param name="maxTreeDepth">The maximum depth of the tree.</param>
        /// <returns>The total number of leaf partitions in the BSP tree.</returns>
        public static int CalculateLeafPartitionCount(int maxTreeDepth) => (int)Mathf.Pow(2, maxTreeDepth);

        /// <summary>
        /// Inserts a cell into the BSP tree.
        /// </summary>
        /// <param name="cell">The cell to insert.</param>
        public void Insert(Cell Cell) => Insert(0, Cell);


        /// <summary>
        /// Inserts a cell into a specific partition in the BSP tree.
        /// </summary>
        /// <param name="partitionIndex">The index of the partition to insert the cell into.</param>
        /// <param name="cell">The cell to be inserted.</param>
        private void Insert(int partitionIndex, Cell cell)
        {
            Partition partition = _partitions[partitionIndex];

            // if this Partition is not a leaf Partition, insert this enemy into the correct child Partition
            if (partition.FirstElementIndex == Null)
            {
                if (partition.SplitAlongXAxis)
                {
                    if (cell.GetPosition().X < partition.SplitValue)
                        Insert(partition.LeftChildIndex, cell);
                    else
                        Insert(partition.RightChildIndex, cell);
                }
                else
                {
                    if (cell.GetPosition().Y < partition.SplitValue)
                        Insert(partition.LeftChildIndex, cell);
                    else
                        Insert(partition.RightChildIndex, cell);
                }
            }
            // else add it to this partition
            else
            {
                CellArray.Add(cell);

                int nextElementIndex = partition.FirstElementIndex == Empty ? Null : partition.FirstElementIndex;
                ElementIndices.Add(new ElementIndex(nextElementIndex, CellArray.Count - 1));
                partition.FirstElementIndex = ElementIndices.Count - 1;

            }
        }

        /// <summary>
        /// Reinserts a cell into a specific partition in the BSPTree.
        /// If the partition is not a leaf partition, the cell is inserted into the correct child partition based on its position.
        /// If the partition is a leaf partition, the cell is added to the partition.
        /// </summary>
        /// <param name="partitionIndex">The index of the partition.</param>
        /// <param name="cell">The cell to be reinserted.</param>
        /// <param name="cellIndex">The index of the cell.</param>
        private void Reinsert(int partitionIndex, Cell cell, int cellIndex)
        {
            Partition partition = _partitions[partitionIndex];

            // if this Partition is not a leaf Partition, insert this cell into the correct child Partition
            if (partition.FirstElementIndex == Null)
            {
                if (partition.SplitAlongXAxis)
                {
                    if (cell.GetPosition().X <= partition.SplitValue)
                        Reinsert(partition.LeftChildIndex, cell, cellIndex);
                    else
                        Reinsert(partition.RightChildIndex, cell, cellIndex);
                }
                else
                {
                    if (cell.GetPosition().Y <= partition.SplitValue)
                        Reinsert(partition.LeftChildIndex, cell, cellIndex);
                    else
                        Reinsert(partition.RightChildIndex, cell, cellIndex);
                }
            }
            // else add it to this partition
            else
            {
                if (partition.FirstElementIndex == Empty)
                {
                    ElementIndices.Add(new ElementIndex(Null, cellIndex));
                    partition.FirstElementIndex = cellIndex;
                }
                else
                {
                    ElementIndices.Add(new ElementIndex(partition.FirstElementIndex, cellIndex));
                    partition.FirstElementIndex = cellIndex;
                }
            }
        }


        /// <summary>
        /// Reinserts all cells into the BSPTree and performs cleaning if needed.
        /// </summary>
        public void ReinsertAllAndCleanIfNeeded()
        {
            if (CellArray.ShouldBeCleaned())
                ReinsertAllAndClean();
        }

        /// <summary>
        /// Reinserts all cells into the BSP tree and remove destroyed cells from the cell array if needed.
        /// </summary>
        public void ReinsertAllAndClean()
        {
            foreach (Partition partition in _leafPartitions)
                partition.FirstElementIndex = Empty;

            ElementIndices.Clear();
            CellArray.Clean();

            int offset = 0;
            for (int i = 0; i < CellArray.Count; i++)
            {
                Cell cell = CellArray.Get(i);
                if (cell.IsDestroyed)
                    offset++;
                else
                    Reinsert(0, cell, i - offset);
            }
        }

        /// <summary>
        /// Increments the count of destroyed cells in the BSP tree.
        /// </summary>
        public void IncrementDestroyedCells() => CellArray.IncrementDestroyedCells();

        /// <summary>
        /// Collects the leaf partitions of the BSP tree up to a specified maximum tree depth.
        /// </summary>
        /// <param name="maxTreeDepth">The maximum depth of the tree to collect leaf partitions from.</param>
        /// <returns>An array of leaf partitions.</returns>
        private Partition[] CollectLeafPartitions(int maxTreeDepth)
        {
            Partition[] leafPartitionList = new Partition[CalculateLeafPartitionCount(maxTreeDepth)];

            Queue<int> partitionIndexQueue = new Queue<int>();
            partitionIndexQueue.Enqueue(0);

            while (partitionIndexQueue.Count > 0)
            {
                Partition currentPartition = _partitions[partitionIndexQueue.Dequeue()];

                if (currentPartition.FirstElementIndex != Null)
                {
                    leafPartitionList[_leafPartitionLastIndex] = currentPartition;
                    _leafPartitionLastIndex++;
                }
                else
                {
                    partitionIndexQueue.Enqueue(currentPartition.RightChildIndex);
                    partitionIndexQueue.Enqueue(currentPartition.LeftChildIndex);
                }
            }

            return leafPartitionList;
        }

        /// <summary>
        /// Finds the positions within shooting range of a given position in the BSP tree.
        /// </summary>
        /// <param name="pos">The position to find other positions in shooting range for.</param>
        /// <param name="partition">The partition to start the search from. If not provided, the root partition is used.</param>
        /// <returns>A list of positions within shooting range of the given position.</returns>
        public List<Pos> FindPosInShootingRange(Pos pos, Func<Cell, bool> CellSelectorFunction = null) => FindPosInShootingRange(pos, _partitions[0], CellSelectorFunction);
        private List<Pos> FindPosInShootingRange(Pos pos, Partition partition, Func<Cell, bool> CellSelectorFunction = null)
        {
            if (CellArray.Count == 0)
                return new List<Pos>();

            // if partition is leaf node, return nearest pos in partition (or null if no cells in partition)
            if (partition.IsLeafNode)
            {
                if (partition.FirstElementIndex == Empty)
                    return new List<Pos>();

                ElementIndex currentIndex = ElementIndices[partition.FirstElementIndex];
                List<Pos> posInShootingRange = new List<Pos>();
                while (currentIndex.Next != -1)
                {
                    Cell currentCell = CellArray.Get(currentIndex.Element);

                    currentIndex = ElementIndices[currentIndex.Next];
                    // skip if cell content no longer exists
                    if (currentCell.IsDestroyed)
                        continue;
                    // skip if cell is not wanted
                    if (!CellSelectorFunction(currentCell))
                        continue;

                    if (pos.InShootingRangeOf(currentCell.GetPosition()))
                    {
                        posInShootingRange.Add(currentCell.GetPosition());
                    }
                }

                if (posInShootingRange.Count == 0)
                    return new List<Pos>();

                return posInShootingRange;
            }

            float splitAxisPosValue = partition.SplitAlongXAxis ? pos.X : pos.Y;
            float splitAxisDistance = Mathf.Abs(splitAxisPosValue - partition.SplitValue);
            bool posIsInLeftChild = splitAxisPosValue <= partition.SplitValue;

            Partition Child1Partition = _partitions[posIsInLeftChild ? partition.LeftChildIndex : partition.RightChildIndex];
            List<Pos> PosInChild1 = FindPosInShootingRange(pos, Child1Partition, CellSelectorFunction);

            if (splitAxisDistance < Bot.ShootingRange)
            {
                Partition Child2Partition = _partitions[posIsInLeftChild ? partition.RightChildIndex : partition.LeftChildIndex];
                List<Pos> PosInChild2 = FindPosInShootingRange(pos, Child2Partition, CellSelectorFunction);

                PosInChild1.AddRange(PosInChild2);
                return PosInChild1;
            }
            return PosInChild1;
        }


        /// <summary>
        /// Finds the nearest position to a given position in the BSP tree.
        /// </summary>
        /// <param name="pos">The position to find the nearest position for.</param>
        /// <param name="partition">The partition to start the search from. If not provided, the root partition is used.</param>
        /// <returns>The nearest position, or null if no positions are found.</returns>
        public Pos FindNearestPos(Pos pos, Func<Cell, bool> CellSelectorFunction = null) => FindNearestPos(pos, _partitions[0], CellSelectorFunction);

        private Pos FindNearestPos(Pos pos, Partition partition, Func<Cell, bool> CellSelectorFunction = null)
        {
            if (CellArray.Count == 0)
                return null;

            CellSelectorFunction ??= x => true;

            // if partition is leaf node, return nearest pos in partition (or null if no cells in partition)
            if (partition.IsLeafNode)
            {
                if (partition.FirstElementIndex == Empty)
                    return null;

                // ElementIndex currentElementIndex = ElementIndices[partition.FirstElementIndex];
                int currentIndex = partition.FirstElementIndex;
                Cell nearestCell = null;
                float nearestDistance = float.MaxValue;
                while (currentIndex != Null)
                {
                    ElementIndex currentElementIndex = ElementIndices[currentIndex];
                    Cell currentCell = CellArray.Get(currentElementIndex.Element);
                    currentIndex = currentElementIndex.Next;

                    // skip if cell content no longer exists
                    if (currentCell.IsDestroyed)
                        continue;

                    // skip if cell is not wanted
                    if (!CellSelectorFunction(currentCell))
                        continue;

                    int currentDistance = pos.DistanceToSquared(currentCell.GetPosition());
                    if (currentDistance < nearestDistance)
                    {
                        nearestCell = currentCell;
                        nearestDistance = currentDistance;
                    }
                }

                return nearestCell?.GetPosition();
            }

            float splitAxisPosValue = partition.SplitAlongXAxis ? pos.X : pos.Y;
            float splitAxisDistance = Mathf.Pow(Mathf.Abs(splitAxisPosValue - partition.SplitValue), 2);
            bool posIsInLeftChild = splitAxisPosValue <= partition.SplitValue;

            Partition Child1Partition = _partitions[posIsInLeftChild ? partition.LeftChildIndex : partition.RightChildIndex];
            Pos nearestPosInChild1 = FindNearestPos(pos, Child1Partition, CellSelectorFunction);

            float nearestPosInChild1Distance = nearestPosInChild1 is not null ?
                                                    pos.DistanceToSquared(nearestPosInChild1) :
                                                    float.MaxValue;

            if (splitAxisDistance < nearestPosInChild1Distance)
            {
                Partition Child2Partition = _partitions[posIsInLeftChild ? partition.RightChildIndex : partition.LeftChildIndex];
                Pos nearestPosInChild2 = FindNearestPos(pos, Child2Partition, CellSelectorFunction);

                float nearestPosInChild2Distance = nearestPosInChild2 is not null ?
                                                         pos.DistanceToSquared(nearestPosInChild2) :
                                                         float.MaxValue;

                if (nearestPosInChild2Distance < nearestPosInChild1Distance)
                    return nearestPosInChild2;
                return nearestPosInChild1;
            }

            return nearestPosInChild1;
        }


        /// <summary>
        /// Finds the nearest X positions to a given position in the BSP tree.
        /// </summary>
        /// <param name="pos">The position to find the nearest X positions for.</param>
        /// <param name="x">The number of nearest positions to find.</param>
        /// <param name="CellSelectorFunction">Optional selector function to filter cells.</param>
        /// <returns>A sorted list of the nearest X positions.</returns>
        public List<Pos> FindXNearestPos(Pos pos, int x = 5, Func<Cell, bool> CellSelectorFunction = null)
        {
            var nearestCells = new List<(Cell cell, float distance)>();
            FindXNearestPos(pos, _partitions[0], x, ref nearestCells, CellSelectorFunction);

            if (nearestCells.Count == 0)
                return null;
            
            nearestCells.Sort((a, b) => a.distance.CompareTo(b.distance));
            return nearestCells.Select(c => c.cell.GetPosition()).ToList();
        }

        private void FindXNearestPos(Pos pos, Partition partition, int x, ref List<(Cell cell, float distance)> nearestCells, Func<Cell, bool> CellSelectorFunction = null)
        {
            if (CellArray.Count == 0)
                return;

            CellSelectorFunction ??= c => true;

            if (partition.IsLeafNode)
            {
                if (partition.FirstElementIndex == Empty)
                    return;

                int currentIndex = partition.FirstElementIndex;
                while (currentIndex != Null)
                {
                    ElementIndex currentElementIndex = ElementIndices[currentIndex];
                    Cell currentCell = CellArray.Get(currentElementIndex.Element);
                    currentIndex = currentElementIndex.Next;

                    if (currentCell.IsDestroyed || !CellSelectorFunction(currentCell))
                        continue;

                    float currentDistance = pos.DistanceToSquared(currentCell.GetPosition());
                    if (nearestCells.Count < x || currentDistance < nearestCells.Last().distance)
                    {
                        nearestCells.Add((currentCell, currentDistance));
                        nearestCells.Sort((a, b) => a.distance.CompareTo(b.distance));

                        if (nearestCells.Count > x)
                            nearestCells.RemoveAt(nearestCells.Count - 1);
                    }
                }
            }
            else
            {
                float splitAxisPosValue = partition.SplitAlongXAxis ? pos.X : pos.Y;
                bool posIsInLeftChild = splitAxisPosValue <= partition.SplitValue;

                Partition Child1Partition = _partitions[posIsInLeftChild ? partition.LeftChildIndex : partition.RightChildIndex];
                FindXNearestPos(pos, Child1Partition, x, ref nearestCells, CellSelectorFunction);

                if (nearestCells.Count < x || Math.Abs(splitAxisPosValue - partition.SplitValue) < Mathf.Sqrt(nearestCells.Last().distance))
                {
                    Partition Child2Partition = _partitions[posIsInLeftChild ? partition.RightChildIndex : partition.LeftChildIndex];
                    FindXNearestPos(pos, Child2Partition, x, ref nearestCells, CellSelectorFunction);
                }
            }
        }



        public List<Pos> GetPosList() => CellArray.ToStandardArray().Select(c => c.GetPosition()).ToList();
    }
}