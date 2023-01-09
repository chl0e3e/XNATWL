/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;

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

        public virtual void RemoveAllChildren()
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
