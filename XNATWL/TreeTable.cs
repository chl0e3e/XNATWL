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
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class TreeTable : TableBase
    {
        private TreeLeafCellRenderer _leafRenderer;
        private TreeNodeCellRenderer _nodeRenderer;

        private Dictionary<TreeTableNode, NodeState> _states;
        TreeTableModel _model;
        private NodeState _rootNodeState;

        public event EventHandler<TreeTableNodeExpandedEventArgs> NodeExpanded;
        public event EventHandler<TreeTableNodeCollapsedEventArgs> NodeCollapsed;

        public TreeTable()
        {
            this._states = new Dictionary<TreeTableNode, NodeState>();
            _leafRenderer = new TreeLeafCellRenderer(this);
            _nodeRenderer = new TreeNodeCellRenderer(this);
            _hasCellWidgetCreators = true;

            ActionMap am = GetOrCreateActionMap();
            am.AddMapping("expandLeadRow", this, "SetLeadRowExpanded", new Object[] { true }, ActionMap.FLAG_ON_PRESSED);
            am.AddMapping("collapseLeadRow", this, "SetLeadRowExpanded", new Object[] { false }, ActionMap.FLAG_ON_PRESSED);
        }

        public TreeTable(TreeTableModel model) : this()
        {
            SetModel(model);
        }

        public virtual void SetModel(TreeTableModel model)
        {
            if (this._model != null)
            {
                this._model.ColumnDeleted -= Model_ColumnDeleted;
                this._model.ColumnInserted -= Model_ColumnInserted;
                this._model.ColumnHeaderChanged -= Model_ColumnHeaderChanged;
                this._model.NodesAdded -= Model_NodesAdded;
                this._model.NodesChanged -= Model_NodesChanged;
                this._model.NodesRemoved -= Model_NodesRemoved;
            }
            this._columnHeaderModel = model;
            this._model = model;
            //this.nodeStateTableSize = 0;
            if (this._model != null)
            {
                this._model.ColumnDeleted += Model_ColumnDeleted;
                this._model.ColumnInserted += Model_ColumnInserted;
                this._model.ColumnHeaderChanged += Model_ColumnHeaderChanged;
                this._model.NodesAdded += Model_NodesAdded;
                this._model.NodesChanged += Model_NodesChanged;
                this._model.NodesRemoved += Model_NodesRemoved;
                this._rootNodeState = CreateNodeState(model);
                this._rootNodeState._level = -1;
                this._rootNodeState._expanded = true;
                this._rootNodeState.InitChildSizes();
                this._numRows = ComputeNumRows();
                this._numColumns = model.Columns;
            }
            else
            {
                this._rootNodeState = null;
                this._numRows = 0;
                this._numColumns = 0;
            }
            ModelAllChanged();
            InvalidateLayout();
        }

        private void Model_NodesRemoved(object sender, TreeNodesChangedEventArgs e)
        {
            ModelNodesRemoved(e.Parent, e.Index, e.Count);
        }

        private void Model_NodesChanged(object sender, TreeNodesChangedEventArgs e)
        {
            ModelNodesChanged(e.Parent, e.Index, e.Count);
        }

        private void Model_NodesAdded(object sender, TreeNodesChangedEventArgs e)
        {
            ModelNodesAdded(e.Parent, e.Index, e.Count);
        }

        private void Model_ColumnHeaderChanged(object sender, ColumnHeaderChangedEventArgs e)
        {
            ModelColumnHeaderChanged(e.Column);
        }

        private void Model_ColumnInserted(object sender, ColumnsChangedEventArgs e)
        {
            _numColumns = _model.Columns;
            ModelColumnsInserted(e.Index, e.Count);
        }

        private void Model_ColumnDeleted(object sender, ColumnsChangedEventArgs e)
        {
            _numColumns = _model.Columns;
            ModelColumnsDeleted(e.Index, e.Count);
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeTreeTable(themeInfo);
        }

        protected void ApplyThemeTreeTable(ThemeInfo themeInfo)
        {
            ApplyCellRendererTheme(_leafRenderer);
            ApplyCellRendererTheme(_nodeRenderer);
        }

        /**
         * Computes the row for the given node in the TreeTable.
         *
         * @param node the node to locate
         * @return the row in the table or -1 if the node is not visible
         */
        public int GetRowFromNode(TreeTableNode node)
        {
            int position = -1;
            TreeTableNode parent = node.Parent;
            while (parent != null)
            {
                if (!this._states.ContainsKey(parent))
                {
                    // parent was not yet expanded, or not part of tree
                    return -1;
                }
                NodeState ns = this._states[parent];
                if (ns == null)
                {
                    return -1;
                }
                int idx = parent.ChildIndexOf(node);
                if (idx < 0)
                {
                    // node is not part of the tree
                    return -1;
                }
                if (ns._childSizes == null)
                {
                    if (ns._expanded)
                    {
                        ns.InitChildSizes();
                    }
                    else
                    {
                        return -1;
                    }
                }
                idx = ns._childSizes.GetPosition(idx);
                position += idx + 1;
                node = parent;
                parent = node.Parent;
            }
            return position;
        }

        public int GetRowFromNodeExpand(TreeTableNode node)
        {
            if (node.Parent != null)
            {
                TreeTableNode parent = node.Parent;
                int row = GetRowFromNodeExpand(parent);
                int idx = parent.ChildIndexOf(node);
                NodeState ns = GetOrCreateNodeState(parent);
                ns.Value = true;
                if (ns._childSizes == null)
                {
                    ns.InitChildSizes();
                }
                return row + 1 + ns._childSizes.GetPosition(idx);
            }
            else
            {
                return -1;
            }
        }

        public override TreeTableNode GetNodeFromRow(int row)
        {
            NodeState ns = _rootNodeState;
            for (; ; )
            {
                int idx;
                if (ns._childSizes == null)
                {
                    idx = Math.Min(ns._key.Children - 1, row);
                    row -= idx + 1;
                }
                else
                {
                    idx = ns._childSizes.GetIndex(row);
                    row -= ns._childSizes.GetPosition(idx) + 1;
                }
                if (row < 0)
                {
                    return ns._key.ChildAt(idx);
                }
                System.Diagnostics.Debug.Assert(ns._children[idx] != null);
                ns = ns._children[idx];
            }
        }

        public void CollapseAll()
        {
            foreach (NodeState ns in this._states.Values)
            {
                if (ns != _rootNodeState)
                {
                    ns.Value = false;
                }
            }
        }

        public bool IsRowExpanded(int row)
        {
            CheckRowIndex(row);
            TreeTableNode node = GetNodeFromRow(row);
            NodeState ns = null;
            if (this._states.ContainsKey(node))
            {
                ns = this._states[node];
            }
            return (ns != null) && ns._expanded;
        }

        public void SetRowExpanded(int row, bool expanded)
        {
            CheckRowIndex(row);
            TreeTableNode node = GetNodeFromRow(row);
            NodeState state = GetOrCreateNodeState(node);
            state.Value = expanded;
        }

        public void SetLeadRowExpanded(bool expanded)
        {
            TableSelectionManager sm = GetSelectionManager();
            if (sm != null)
            {
                int row = sm.GetLeadRow();
                if (row >= 0 && row < _numRows)
                {
                    SetRowExpanded(row, expanded);
                }
            }
        }

        protected NodeState GetOrCreateNodeState(TreeTableNode node)
        {
            NodeState ns;
            if (!this._states.ContainsKey(node))
            {
                ns = CreateNodeState(node);
            }
            else
            {
                ns = this._states[node];
            }
            return ns;
        }

        protected NodeState CreateNodeState(TreeTableNode node)
        {
            TreeTableNode parent = node.Parent;
            NodeState nsParent = null;
            if (parent != null)
            {
                System.Diagnostics.Debug.Assert(this._states.ContainsKey(parent));
                nsParent = this._states[parent];
            }
            NodeState newNS = new NodeState(this, node, nsParent);
            this._states.Add(node, newNS);
            return newNS;
        }

        protected void ExpandedChanged(NodeState ns)
        {
            TreeTableNode node = ns._key;
            int count = ns.GetChildRows();
            int size = ns._expanded ? count : 0;

            TreeTableNode parent = node.Parent;
            while (parent != null)
            {
                NodeState nsParent = this._states[parent];
                if (nsParent._childSizes == null)
                {
                    nsParent.InitChildSizes();
                }

                int idx = nsParent._key.ChildIndexOf(node);
                nsParent._childSizes.SetSize(idx, size + 1);
                size = nsParent._childSizes.GetEndPosition();

                node = parent;
                parent = node.Parent;
            }

            _numRows = ComputeNumRows();
            int row = GetRowFromNode(ns._key);
            if (ns._expanded)
            {
                ModelRowsInserted(row + 1, count);
            }
            else
            {
                ModelRowsDeleted(row + 1, count);
            }
            ModelRowsChanged(row, 1);

            if (ns._expanded)
            {
                ScrollPane scrollPane = ScrollPane.GetContainingScrollPane(this);
                if (scrollPane != null)
                {
                    scrollPane.ValidateLayout();
                    int rowStart = GetRowStartPosition(row);
                    int rowEnd = GetRowEndPosition(row + count);
                    int height = rowEnd - rowStart;
                    scrollPane.ScrollToAreaY(rowStart, height, _rowHeight / 2);
                }
            }

            if (ns._expanded)
            {
                if (this.NodeExpanded != null)
                {
                    this.NodeExpanded.Invoke(this, new TreeTableNodeExpandedEventArgs(row, ns._key));
                }
            }
            else
            {
                if (this.NodeCollapsed != null)
                {
                    this.NodeCollapsed.Invoke(this, new TreeTableNodeCollapsedEventArgs(row, ns._key));
                }
            }
        }

        protected int ComputeNumRows()
        {
            return _rootNodeState._childSizes.GetEndPosition();
        }

        public override Object GetCellData(int row, int column, TreeTableNode node)
        {
            if (node == null)
            {
                node = GetNodeFromRow(row);
            }
            return node.DataAtColumn(column);
        }

        protected override CellRenderer GetCellRenderer(int row, int col, TreeTableNode node)
        {
            if (node == null)
            {
                node = GetNodeFromRow(row);
            }
            if (col == 0)
            {
                Object data = node.DataAtColumn(col);
                if (node.IsLeaf)
                {
                    _leafRenderer.SetCellData(row, col, data, node);
                    return _leafRenderer;
                }
                NodeState nodeState = GetOrCreateNodeState(node);
                _nodeRenderer.SetCellData(row, col, data, nodeState);
                return _nodeRenderer;
            }
            return base.GetCellRenderer(row, col, node);
        }

        public override Object GetTooltipContentFromRow(int row, int column)
        {
            TreeTableNode node = GetNodeFromRow(row);
            if (node != null)
            {
                return node.TooltipContentAtColumn(column);
            }
            return null;
        }

        private bool UpdateParentSizes(NodeState ns)
        {
            while (ns._expanded && ns._parent != null)
            {
                NodeState parent = ns._parent;
                int idx = parent._key.ChildIndexOf(ns._key);
                System.Diagnostics.Debug.Assert(parent._childSizes._size == parent._key.Children);
                parent._childSizes.SetSize(idx, ns.GetChildRows() + 1);
                ns = parent;
            }
            _numRows = ComputeNumRows();
            return ns._parent == null;
        }

        protected void ModelNodesAdded(TreeTableNode parent, int idx, int count)
        {
            if (!this._states.ContainsKey(parent))
            {
                return;
            }

            NodeState ns = this._states[parent];
            // if ns is null then this node has not yet been displayed
            if (ns != null)
            {
                if (ns._childSizes != null)
                {
                    System.Diagnostics.Debug.Assert(idx <= ns._childSizes._size);
                    ns._childSizes.Insert(idx, count);
                    System.Diagnostics.Debug.Assert(ns._childSizes._size == parent.Children);
                }
                if (ns._children != null)
                {
                    NodeState[] newChilds = new NodeState[parent.Children];
                    Array.Copy(ns._children, 0, newChilds, 0, idx);
                    Array.Copy(ns._children, idx, newChilds, idx + count, ns._children.Length - idx);
                    ns._children = newChilds;
                }
                if (UpdateParentSizes(ns))
                {
                    int row = GetRowFromNode(parent.ChildAt(idx));
                    System.Diagnostics.Debug.Assert(row < _numRows);
                    ModelRowsInserted(row, count);
                }
            }
        }

        protected void RecursiveRemove(NodeState ns)
        {
            if (ns != null)
            {
                //--nodeStateTableSize;
                this._states.Remove(ns._key);
                if (ns._children != null)
                {
                    foreach (NodeState nsChild in ns._children)
                    {
                        RecursiveRemove(nsChild);
                    }
                }
            }
        }

        protected void ModelNodesRemoved(TreeTableNode parent, int idx, int count)
        {
            NodeState ns = null;
            // if ns is null then this node has not yet been displayed
            if (this._states.ContainsKey(parent))
            {
                ns = this._states[parent];
            }
            if (ns != null)
            {
                int rowsBase = GetRowFromNode(parent) + 1;
                int rowsStart = rowsBase + idx;
                int rowsEnd = rowsBase + idx + count;
                if (ns._childSizes != null)
                {
                    System.Diagnostics.Debug.Assert(ns._childSizes._size == parent.Children + count);
                    rowsStart = rowsBase + ns._childSizes.GetPosition(idx);
                    rowsEnd = rowsBase + ns._childSizes.GetPosition(idx + count);
                    ns._childSizes.Remove(idx, count);
                    System.Diagnostics.Debug.Assert(ns._childSizes._size == parent.Children);
                }
                if (ns._children != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        RecursiveRemove(ns._children[idx + i]);
                    }
                    int numChildren = parent.Children;
                    if (numChildren > 0)
                    {
                        NodeState[] newChilds = new NodeState[numChildren];
                        Array.Copy(ns._children, 0, newChilds, 0, idx);
                        Array.Copy(ns._children, idx + count, newChilds, idx, newChilds.Length - idx);
                        ns._children = newChilds;
                    }
                    else
                    {
                        ns._children = null;
                    }
                }
                if (UpdateParentSizes(ns))
                {
                    ModelRowsDeleted(rowsStart, rowsEnd - rowsStart);
                }
            }
        }

        protected bool IsVisible(NodeState ns)
        {
            while (ns._expanded && ns._parent != null)
            {
                ns = ns._parent;
            }
            return ns._expanded;
        }

        protected void ModelNodesChanged(TreeTableNode parent, int idx, int count)
        {
            NodeState ns = null;
            // if ns is null then this node has not yet been displayed
            if (this._states.ContainsKey(parent))
            {
                ns = this._states[parent];
            }
            // if ns is null then this node has not yet been displayed
            if (ns != null && IsVisible(ns))
            {
                int rowsBase = GetRowFromNode(parent) + 1;
                int rowsStart = rowsBase + idx;
                int rowsEnd = rowsBase + idx + count;
                if (ns._childSizes != null)
                {
                    rowsStart = rowsBase + ns._childSizes.GetPosition(idx);
                    rowsEnd = rowsBase + ns._childSizes.GetPosition(idx + count);
                }
                ModelRowsChanged(rowsStart, rowsEnd - rowsStart);
            }
        }

        public class NodeState : BooleanModel
        {
            protected internal NodeState _parent;
            protected internal bool _expanded;
            protected internal bool _bHasNoChildren;
            protected internal SizeSequence _childSizes;
            protected internal NodeState[] _children;
            protected internal int _level;
            protected internal TreeTableNode _key;

            protected internal TreeTable _treeTable;

            public bool Value
            {
                get
                {
                    return _expanded;
                }
                set
                {
                    bool old = this._expanded;
                    if (this._expanded != value)
                    {
                        this._expanded = value;
                        this._treeTable.ExpandedChanged(this);
                        this.Changed.Invoke(this, new BooleanChangedEventArgs(old, this._expanded));
                    }
                }
            }

            public event EventHandler<BooleanChangedEventArgs> Changed;

            public NodeState(TreeTable treeTable, TreeTableNode key, NodeState parent)
            {
                this._treeTable = treeTable;
                this._key = key;
                this._parent = parent;
                this._level = (parent != null) ? parent._level + 1 : 0;

                if (parent != null)
                {
                    if (parent._children == null)
                    {
                        parent._children = new NodeState[parent._key.Children];
                    }
                    parent._children[parent._key.ChildIndexOf(key)] = this;
                }
            }

            protected internal void InitChildSizes()
            {
                _childSizes = new SizeSequence();
                _childSizes.SetDefaultValue(1);
                _childSizes.InitializeAll(_key.Children);
            }

            protected internal int GetChildRows()
            {
                if (_childSizes != null)
                {
                    return _childSizes.GetEndPosition();
                }
                int childCount = _key.Children;
                _bHasNoChildren = childCount == 0;
                return childCount;
            }

            protected internal bool HasNoChildren()
            {
                return _bHasNoChildren;
            }
        }

        static int GetLevel(TreeTableNode node)
        {
            int level = -2;
            while (node != null)
            {
                level++;
                node = node.Parent;
            }
            return level;
        }

        public class TreeLeafCellRenderer : CellRenderer, CellWidgetCreator
        {
            protected int _treeIndent;
            protected int _level;
            protected Dimension _treeButtonSize = new Dimension(5, 5);
            protected CellRenderer _subRenderer;
            private TreeTable _treeTable;

            public TreeLeafCellRenderer(TreeTable treeTable)
            {
                this._treeTable = treeTable;
                this._treeTable.SetClip(true);
            }

            public void ApplyTheme(ThemeInfo themeInfo)
            {
                _treeIndent = themeInfo.GetParameter("treeIndent", 10);
                _treeButtonSize = themeInfo.GetParameterValue("treeButtonSize", true, typeof(Dimension), Dimension.ZERO);
            }

            public String GetTheme()
            {
                return GetType().Name;
            }

            public void SetCellData(int row, int column, Object data)
            {
                throw new InvalidOperationException("Don't call this method");
            }

            public void SetCellData(int row, int column, Object data, TreeTableNode node)
            {
                _level = GetLevel(node);
                SetSubRenderer(row, column, data);
            }

            protected int GetIndentation()
            {
                return _level * _treeIndent + _treeButtonSize.X;
            }

            protected virtual void SetSubRenderer(int row, int column, Object colData)
            {
                _subRenderer = this._treeTable.GetCellRenderer(colData, column);
                if (_subRenderer != null)
                {
                    _subRenderer.SetCellData(row, column, colData);
                }
            }

            public virtual int GetColumnSpan()
            {
                return (_subRenderer != null) ? _subRenderer.GetColumnSpan() : 1;
            }

            public int GetPreferredHeight()
            {
                if (_subRenderer != null)
                {
                    return Math.Max(_treeButtonSize.Y, _subRenderer.GetPreferredHeight());
                }
                return _treeButtonSize.Y;
            }

            public virtual Widget GetCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                if (_subRenderer != null)
                {
                    int indent = GetIndentation();
                    Widget widget = _subRenderer.GetCellRenderWidget(
                            x + indent, y, Math.Max(0, width - indent), height, isSelected);
                    return widget;
                }
                return null;
            }

            public virtual Widget UpdateWidget(Widget existingWidget)
            {
                if (_subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)_subRenderer;
                    return subCreator.UpdateWidget(existingWidget);
                }
                return null;
            }

            public virtual void PositionWidget(Widget widget, int x, int y, int w, int h)
            {
                if (_subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)_subRenderer;
                    int indent = _level * _treeIndent;
                    subCreator.PositionWidget(widget, x + indent, y, Math.Max(0, w - indent), h);
                }
            }
        }

        class WidgetChain : Widget
        {
            protected internal ToggleButton _expandButton;
            protected internal Widget _userWidget;

            protected internal WidgetChain()
            {
                SetTheme("");
                _expandButton = new ToggleButton();
                _expandButton.SetTheme("treeButton");
                Add(_expandButton);
            }

            protected internal void SetUserWidget(Widget userWidget)
            {
                if (this._userWidget != userWidget)
                {
                    if (this._userWidget != null)
                    {
                        RemoveChild(1);
                    }
                    this._userWidget = userWidget;
                    if (userWidget != null)
                    {
                        InsertChild(userWidget, 1);
                    }
                }
            }
        }

        public class TreeNodeCellRenderer : TreeLeafCellRenderer
        {
            private NodeState nodeState;

            public TreeNodeCellRenderer(TreeTable treeTable) : base(treeTable)
            {

            }

            public override Widget UpdateWidget(Widget existingWidget)
            {
                if (_subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)_subRenderer;
                    WidgetChain widgetChain = null;
                    if (existingWidget is WidgetChain)
                    {
                        widgetChain = (WidgetChain)existingWidget;
                    }
                    if (nodeState.HasNoChildren())
                    {
                        if (widgetChain != null)
                        {
                            existingWidget = null;
                        }
                        return subCreator.UpdateWidget(existingWidget);
                    }
                    if (widgetChain == null)
                    {
                        widgetChain = new WidgetChain();
                    }
                    widgetChain._expandButton.SetModel(nodeState);
                    widgetChain.SetUserWidget(subCreator.UpdateWidget(widgetChain._userWidget));
                    return widgetChain;
                }
                if (nodeState.HasNoChildren())
                {
                    return null;
                }
                ToggleButton tb = (ToggleButton)existingWidget;
                if (tb == null)
                {
                    tb = new ToggleButton();
                    tb.SetTheme("treeButton");
                }
                tb.SetModel(nodeState);
                return tb;
            }

            public override void PositionWidget(Widget widget, int x, int y, int w, int h)
            {
                int indent = _level * _treeIndent;
                int availWidth = Math.Max(0, w - indent);
                int expandButtonWidth = Math.Min(availWidth, _treeButtonSize.X);
                widget.SetPosition(x + indent, y + (h - _treeButtonSize.Y) / 2);
                if (_subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)_subRenderer;
                    WidgetChain widgetChain = (WidgetChain)widget;
                    ToggleButton expandButton = widgetChain._expandButton;
                    widgetChain.SetSize(Math.Max(0, w - indent), h);
                    expandButton.SetSize(expandButtonWidth, _treeButtonSize.Y);
                    if (widgetChain._userWidget != null)
                    {
                        subCreator.PositionWidget(widgetChain._userWidget,
                                expandButton.GetRight(), y, widget.GetWidth(), h);
                    }
                }
                else
                {
                    widget.SetSize(expandButtonWidth, _treeButtonSize.Y);
                }
            }

            public virtual void SetCellData(int row, int column, Object data, NodeState nodeState)
            {
                System.Diagnostics.Debug.Assert(nodeState != null);
                this.nodeState = nodeState;
                SetSubRenderer(row, column, data);
                _level = nodeState._level;
            }
        }
    }

    public class TreeTableNodeChangedEventArgs : EventArgs
    {
    }

    public class TreeTableNodeCollapsedEventArgs : EventArgs
    {
        public int Row;
        public TreeTableNode Node;
        public TreeTableNodeCollapsedEventArgs(int row, TreeTableNode node)
        {
            this.Row = row;
            this.Node = node;
        }
    }

    public class TreeTableNodeExpandedEventArgs : EventArgs
    {
        public int Row;
        public TreeTableNode Node;
        public TreeTableNodeExpandedEventArgs(int row, TreeTableNode node)
        {
            this.Row = row;
            this.Node = node;
        }
    }
}
