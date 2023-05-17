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
    /// <summary>
    /// Abstract class for implementing tree table models
    /// </summary>
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
            if (this.NodesAdded != null)
            {
                this.NodesAdded.Invoke(this, new TreeNodesChangedEventArgs(parent, idx, count));
            }
        }

        internal void FireNodesRemoved(TreeTableNode parent, int idx, int count)
        {
            if (this.NodesRemoved != null)
            {
                this.NodesRemoved.Invoke(this, new TreeNodesChangedEventArgs(parent, idx, count));
            }
        }

        internal void FireNodesChanged(TreeTableNode parent, int idx, int count)
        {
            if (this.NodesChanged != null)
            {
                this.NodesChanged.Invoke(this, new TreeNodesChangedEventArgs(parent, idx, count));
            }
        }
    }
}