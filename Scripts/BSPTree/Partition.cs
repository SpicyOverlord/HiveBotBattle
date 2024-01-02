using System;

namespace HiveBotBattle.Scripts.BSPTree
{
    /// <summary>
    /// Represents a partition in a Binary Space Partition (BSP) tree.
    /// </summary>
    public class Partition
    {
        /// <summary>
        /// The bounds of the partition.
        /// </summary>
        public readonly Bounds2D Bounds;

        /// <summary>
        /// The value used to split the partition.
        /// Normally the middle of the partition's bounds, so the children are equal in size.
        /// If the partition is split along the X-axis, this is the x-coordinate of the split.
        /// If the partition is split along the Y-axis, this is the y-coordinate of the split.
        /// </summary>
        public int SplitValue;

        /// <summary>
        /// Indicates whether the partition is split along the X-axis.
        /// </summary>
        public bool SplitAlongXAxis;

        /// <summary>
        /// Indicates whether the partition is a leaf node.
        /// </summary>
        public bool IsLeafNode => FirstElementIndex != BSPTree.Null;

        private int _leftChildIndex, _rightChildIndex;

        /// <summary>
        /// The index of the left child partition.
        /// </summary>
        public int LeftChildIndex => _leftChildIndex;

        /// <summary>
        /// The index of the right child partition.
        /// </summary>
        public int RightChildIndex => _rightChildIndex;

        /// <summary>
        /// The depth of the partition in the BSP tree.
        /// </summary>
        public readonly int PartitionDepthInTree;

        /// <summary>
        /// The index of the first element in the partition.
        /// </summary>
        public int FirstElementIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Partition"/> class.
        /// </summary>
        /// <param name="bounds">The bounds of the partition.</param>
        /// <param name="partitionDepthInTree">The depth of the partition in the BSP tree.</param>
        public Partition(Bounds2D bounds, int partitionDepthInTree = 0)
        {
            FirstElementIndex = BSPTree.Null;

            Bounds = bounds;
            PartitionDepthInTree = partitionDepthInTree;

            SplitAlongXAxis = PartitionDepthInTree % 2 == 0;

            // calculate split value
            float diff = SplitAlongXAxis ? Bounds.xMax - Bounds.xMin : Bounds.yMax - Bounds.yMin;
            int min = SplitAlongXAxis ? Bounds.xMin : Bounds.yMin;
            double splitValue = diff / 2f + min;
            SplitValue = (int)Math.Floor(splitValue);
        }

        /// <summary>
        /// Sets the index of the right child partition.
        /// </summary>
        /// <param name="index">The index of the right child partition.</param>
        public void SetRightChildIndex(int index) => _rightChildIndex = index;

        /// <summary>
        /// Sets the index of the left child partition.
        /// </summary>
        /// <param name="index">The index of the left child partition.</param>
        public void SetLeftChildIndex(int index) => _leftChildIndex = index;
    }
}