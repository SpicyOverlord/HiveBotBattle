namespace HiveBotBattle.Scripts.BSPTree
{
    /// <summary>
    /// Represents an index of an element in a BSP tree.
    /// Used to create a linked-list of elements in a BSP tree partition.
    /// </summary>
    public struct ElementIndex
    {
        /// <summary>
        /// The index of the next element.
        /// </summary>
        public int Next;

        /// <summary>
        /// The index of the element.
        /// </summary>
        public int Element;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementIndex"/> struct.
        /// </summary>
        /// <param name="next">The index of the next element.</param>
        /// <param name="element">The index of the element.</param>
        public ElementIndex(int next, int element)
        {
            Next = next;
            Element = element;
        }
    }
}
