using System.Collections.Generic;
using System.Linq;
using Godot;
using Utils;
using static Map;

namespace HiveBotBattle.Scripts.BSPTree
{
    public class BSPTree
    {
        public const int Null = -1;
        public const int Empty = -2;

        private readonly Partition[] _partitions;
        private int _partitionsLastIndex;
        private readonly Partition[] _leafPartitions;
        private int _leafPartitionLastIndex;

        public readonly List<ElementIndex> ElementIndices;

        public readonly ResizingCellArray CellArray;

        public BSPTree(int width, int height, int TreeDepth)
        {
            ElementIndices = new List<ElementIndex>();
            CellArray = new ResizingCellArray();
            _partitions = new Partition[CalculatePartitionCount(TreeDepth)];

            Bounds2D bounds = new Bounds2D(0,0,width,height);

            _partitions[_partitionsLastIndex] = new Partition(bounds);
            _partitionsLastIndex++;

            CreateTreePartitions(0, TreeDepth);

            _leafPartitions = CollectLeafPartitions(TreeDepth);
        }

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

        public static int CalculatePartitionCount(int maxTreeDepth) => (int)Mathf.Pow(2, maxTreeDepth + 1) - 1;
        public static int CalculateLeafPartitionCount(int maxTreeDepth) => (int)Mathf.Pow(2, maxTreeDepth);

        public void Insert(Cell Cell) => Insert(0, Cell);

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

        public void ReinsertAllAndCleanIfNeeded()
        {
            if (CellArray.Count == 0)
                return;

            foreach (Partition partition in _leafPartitions)
                partition.FirstElementIndex = Empty;


            ElementIndices.Clear();
            CellArray.CleanIfNeeded();

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

        private void Reinsert(int partitionIndex, Cell cell, int cellIndex)
        {
            Partition partition = _partitions[partitionIndex];

            // if this Partition is not a leaf Partition, insert this enemy into the correct child Partition
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

        public void IncrementDestroyedCells() => CellArray.IncrementDestroyedCells();

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

        public List<Pos> FindPosInShootingRange(Pos pos, Partition partition = null)
        {
            if (CellArray.Count == 0)
                return new List<Pos>();

            partition ??= _partitions[0];

            // if partition is leaf node, return nearest pos in partition (or null if no cells in partition)
            if (partition.IsLeafNode)
            {
                if (partition.FirstElementIndex == Empty)
                    return null;

                ElementIndex currentIndex = ElementIndices[partition.FirstElementIndex];
                List<Pos> posInShootingRange = new List<Pos>();
                while (currentIndex.Next != -1)
                {
                    Cell currentCell = CellArray.Get(currentIndex.Element);

                    currentIndex = ElementIndices[currentIndex.Next];
                    // skip if cell content no longer exists
                    if (currentCell.IsDestroyed)
                        continue;

                    if (pos.InShootingRangeOf(currentCell.GetPosition()))
                    {
                        posInShootingRange.Add(currentCell.GetPosition());
                    }
                }

                if (posInShootingRange.Count == 0)
                    return null;

                return posInShootingRange;
            }

            float splitAxisPosValue = partition.SplitAlongXAxis ? pos.X : pos.Y;
            float splitAxisDistance = Mathf.Abs(splitAxisPosValue - partition.SplitValue);
            bool posIsInLeftChild = splitAxisPosValue <= partition.SplitValue;

            Partition Child1Partition = _partitions[posIsInLeftChild ? partition.LeftChildIndex : partition.RightChildIndex];
            List<Pos> PosInChild1 = FindPosInShootingRange(pos, Child1Partition);

            if (splitAxisDistance < Bot.ShootingRange)
            {
                Partition Child2Partition = _partitions[posIsInLeftChild ? partition.RightChildIndex : partition.LeftChildIndex];
                List<Pos> PosInChild2 = FindPosInShootingRange(pos, Child2Partition);

                PosInChild1.AddRange(PosInChild2);
                return PosInChild1;
            }
            return PosInChild1;
        }

        public Pos FindNearestPos(Pos pos, Partition partition = null)
        {
            if (CellArray.Count == 0)
                return null;

            partition ??= _partitions[0];

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
            Pos nearestPosInChild1 = FindNearestPos(pos, Child1Partition);

            float nearestPosInChild1Distance = nearestPosInChild1 is not null ?
                                                    pos.DistanceToSquared((Pos)nearestPosInChild1) :
                                                    float.MaxValue;

            if (splitAxisDistance < nearestPosInChild1Distance)
            {
                Partition Child2Partition = _partitions[posIsInLeftChild ? partition.RightChildIndex : partition.LeftChildIndex];
                Pos nearestPosInChild2 = FindNearestPos(pos, Child2Partition);

                float nearestPosInChild2Distance = nearestPosInChild2 is not null ?
                                                         pos.DistanceToSquared(nearestPosInChild2) :
                                                         float.MaxValue;

                if (nearestPosInChild2Distance < nearestPosInChild1Distance)
                    return nearestPosInChild2;
                return nearestPosInChild1;
            }

            return nearestPosInChild1;
        }

        public List<Pos> GetPosList() => CellArray.ToStandardArray().Select(c => c.GetPosition()).ToList();
    }
}