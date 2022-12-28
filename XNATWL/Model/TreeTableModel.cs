using System;

namespace XNATWL.Model
{
    public interface TreeTableModel : TableColumnHeaderModel, TreeTableNode
    {
        event EventHandler<TreeNodesChangedEventArgs> NodesAdded;

        event EventHandler<TreeNodesChangedEventArgs> NodesRemoved;

        event EventHandler<TreeNodesChangedEventArgs> NodesChanged;
    }

    public class TreeNodesChangedEventArgs : EventArgs
    {
        public TreeTableNode Parent;
        public int Index;
        public int Count;

        public TreeNodesChangedEventArgs(TreeTableNode parent, int idx, int count)
        {
            this.Parent = parent;
            this.Index = idx;
            this.Count = count;
        }
    }
}