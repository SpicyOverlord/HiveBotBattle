namespace HiveBotBattle.Scripts.BSPTree
{
    public struct ElementIndex
    {
        // Points to the next element in the leaf node. A value of -1 
        // indicates the end of the list.
        public int Next;

        // Stores the element index.
        public int Element;
        public ElementIndex(int next, int element)
        {
            Next = next;
            Element = element;
        }
    };
}