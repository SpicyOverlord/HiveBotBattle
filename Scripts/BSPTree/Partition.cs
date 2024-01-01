using System;

namespace HiveBotBattle.Scripts.BSPTree
{
    public class Partition
    {
        public readonly Bounds2D Bounds;
        public int SplitValue;
        public bool SplitAlongXAxis;
        public bool IsLeafNode => FirstElementIndex != BSPTree.Null;

        private int _leftChildIndex, _rightChildIndex;
        public int LeftChildIndex => _leftChildIndex;
        public int RightChildIndex => _rightChildIndex;

        public readonly int PartitionDepthInTree;
        public int FirstElementIndex;

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

        public void SetRightChildIndex(int index) => _rightChildIndex = index;
        public void SetLeftChildIndex(int index) => _leftChildIndex = index;
    }
}