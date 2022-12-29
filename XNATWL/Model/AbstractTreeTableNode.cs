using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class AbstractTreeTableNode : TreeTableNode
    {
        private TreeTableNode _parent;
        private List<TreeTableNode> _children;
        private bool _leaf;

        public TreeTableNode Parent
        {
            get
            {
                return _parent;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return _leaf;
            }
            set
            {
                this._leaf = value;
                this.FireNodeChanged();
            }
        }

        public int Children
        {
            get
            {
                return (_children != null) ? _children.Count : 0;
            }
        }

        protected AbstractTreeTableNode(TreeTableNode parent)
        {
            this._parent = parent;
        }

        public TreeTableNode ChildAt(int idx)
        {
            return this._children[idx];
        }

        public int ChildIndexOf(TreeTableNode child)
        {
            return this._children.IndexOf(child);
        }

        public object DataAtColumn(int column)
        {
            throw new NotImplementedException();
        }

        public object TooltipContentAtColumn(int column)
        {
            return null;
        }

        protected void InsertChild(TreeTableNode node, int idx)
        {
            if (_children == null)
            {
                _children = new List<TreeTableNode>();
            }
            _children.Insert(idx, node);
            this.Model.FireNodesAdded(this, idx, 1);
        }

        protected void RemoveChild(int idx)
        {
            _children.RemoveAt(idx);
            this.Model.FireNodesRemoved(this, idx, 1);
        }

        protected void RemoveAllChildren()
        {
            if (_children != null)
            {
                int count = _children.Count;
                _children.Clear();
                this.Model.FireNodesRemoved(this, 0, count);
            }
        }

        protected AbstractTreeTableModel Model
        {
            get
            {
                TreeTableNode n = _parent;
                for (; ; )
                {
                    TreeTableNode p = n.Parent;
                    if (p == null)
                    {
                        return (AbstractTreeTableModel)n;
                    }
                    n = p;
                }
            }
        }

        protected void FireNodeChanged()
        {
            int selfIdxInParent = _parent.ChildIndexOf(this);
            if (selfIdxInParent >= 0)
            {
                // a negative index means that we are not yet added to the parent
                this.Model.FireNodesChanged(_parent, selfIdxInParent, 1);
            }
        }
    }
}
