using System;
using System.Collections.Generic;

namespace XNATWL.Model
{
    public abstract class AbstractTreeTableModel : AbstractTableColumnHeaderModel, TreeTableModel
    {
        private List<TreeTableNode> _children;

        public event EventHandler<TreeNodesChangedEventArgs> NodesAdded;
        public event EventHandler<TreeNodesChangedEventArgs> NodesRemoved;
        public event EventHandler<TreeNodesChangedEventArgs> NodesChanged;

        public AbstractTreeTableModel()
        {
            this._children = new List<TreeTableNode>();
        }

        public virtual TreeTableNode Parent
        { 
            get
            {
                return null;
            }
        }

        public virtual bool IsLeaf
        {
            get
            {
                return false;
            }
        }

        public virtual int Children
        {
            get
            {
                return _children.Count;
            }
        }

        public virtual TreeTableNode ChildAt(int idx)
        {
            return this._children[idx];
        }

        public virtual int ChildIndexOf(TreeTableNode child)
        {
            return this._children.IndexOf(child);
        }

        public virtual object DataAtColumn(int column)
        {
            return null;
        }

        public virtual object TooltipContentAtColumn(int column)
        {
            return null;
        }

        public void InsertChildAt(TreeTableNode node, int idx)
        {
            if (this.ChildIndexOf(node) >= 0)
            {
                throw new ArgumentOutOfRangeException("node index was not found");
            }

            if (node.Parent != this)
            {
                throw new ArgumentOutOfRangeException("node parent must be this model");
            }

            this._children.Insert(idx, node);

            FireNodesAdded(this, idx, 1);
        }

        public void RemoveChildAt(int idx)
        {
            this._children.RemoveAt(idx);

            FireNodesRemoved(this, idx, 1);
        }

        public virtual void RemoveAllChildren()
        {
            int count = this._children.Count;
            if (count > 0)
            {
                this._children.Clear();

                FireNodesRemoved(this, 0, count);
            }
        }

        internal void FireNodesAdded(TreeTableNode parent, int idx, int count)
        {
            this.NodesAdded.Invoke(this, new TreeNodesChangedEventArgs(parent, idx, count));
        }

        internal void FireNodesRemoved(TreeTableNode parent, int idx, int count)
        {
            this.NodesRemoved.Invoke(this, new TreeNodesChangedEventArgs(parent, idx, count));
        }

        internal void FireNodesChanged(TreeTableNode parent, int idx, int count)
        {
            this.NodesChanged.Invoke(this, new TreeNodesChangedEventArgs(parent, idx, count));
        }
    }
}