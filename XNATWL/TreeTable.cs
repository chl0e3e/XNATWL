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
        private TreeLeafCellRenderer leafRenderer;
        private TreeNodeCellRenderer nodeRenderer;

        private Dictionary<TreeTableNode, NodeState> _states;
        private int nodeStateTableSize;
        TreeTableModel model;
        private NodeState rootNodeState;

        public event EventHandler<TreeTableNodeExpandedEventArgs> NodeExpanded;
        public event EventHandler<TreeTableNodeCollapsedEventArgs> NodeCollapsed;

        public TreeTable()
        {
            this._states = new Dictionary<TreeTableNode, NodeState>();
            leafRenderer = new TreeLeafCellRenderer(this);
            nodeRenderer = new TreeNodeCellRenderer(this);
            hasCellWidgetCreators = true;

            ActionMap am = getOrCreateActionMap();
            am.addMapping("expandLeadRow", this, "setLeadRowExpanded", new Object[] { true }, ActionMap.FLAG_ON_PRESSED);
            am.addMapping("collapseLeadRow", this, "setLeadRowExpanded", new Object[] { false }, ActionMap.FLAG_ON_PRESSED);
        }

        public TreeTable(TreeTableModel model) : this()
        {
            setModel(model);
        }

        public virtual void setModel(TreeTableModel model)
        {
            if (this.model != null)
            {
                this.model.ColumnDeleted -= Model_ColumnDeleted;
                this.model.ColumnInserted -= Model_ColumnInserted;
                this.model.ColumnHeaderChanged -= Model_ColumnHeaderChanged;
                this.model.NodesAdded -= Model_NodesAdded;
                this.model.NodesChanged -= Model_NodesChanged;
                this.model.NodesRemoved -= Model_NodesRemoved;
            }
            this.columnHeaderModel = model;
            this.model = model;
            this.nodeStateTableSize = 0;
            if (this.model != null)
            {
                this.model.ColumnDeleted += Model_ColumnDeleted;
                this.model.ColumnInserted += Model_ColumnInserted;
                this.model.ColumnHeaderChanged += Model_ColumnHeaderChanged;
                this.model.NodesAdded += Model_NodesAdded;
                this.model.NodesChanged += Model_NodesChanged;
                this.model.NodesRemoved += Model_NodesRemoved;
                this.rootNodeState = createNodeState(model);
                this.rootNodeState.level = -1;
                this.rootNodeState.expanded = true;
                this.rootNodeState.initChildSizes();
                this.numRows = computeNumRows();
                this.numColumns = model.Columns;
            }
            else
            {
                this.rootNodeState = null;
                this.numRows = 0;
                this.numColumns = 0;
            }
            modelAllChanged();
            invalidateLayout();
        }

        private void Model_NodesRemoved(object sender, TreeNodesChangedEventArgs e)
        {
            modelNodesRemoved(e.Parent, e.Index, e.Count);
        }

        private void Model_NodesChanged(object sender, TreeNodesChangedEventArgs e)
        {
            modelNodesChanged(e.Parent, e.Index, e.Count);
        }

        private void Model_NodesAdded(object sender, TreeNodesChangedEventArgs e)
        {
            modelNodesAdded(e.Parent, e.Index, e.Count);
        }

        private void Model_ColumnHeaderChanged(object sender, ColumnHeaderChangedEventArgs e)
        {
            modelColumnHeaderChanged(e.Column);
        }

        private void Model_ColumnInserted(object sender, ColumnsChangedEventArgs e)
        {
            numColumns = model.Columns;
            modelColumnsInserted(e.Index, e.Count);
        }

        private void Model_ColumnDeleted(object sender, ColumnsChangedEventArgs e)
        {
            numColumns = model.Columns;
            modelColumnsDeleted(e.Index, e.Count);
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeTreeTable(themeInfo);
        }

        protected void applyThemeTreeTable(ThemeInfo themeInfo)
        {
            applyCellRendererTheme(leafRenderer);
            applyCellRendererTheme(nodeRenderer);
        }

        /**
         * Computes the row for the given node in the TreeTable.
         *
         * @param node the node to locate
         * @return the row in the table or -1 if the node is not visible
         */
        public int getRowFromNode(TreeTableNode node)
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
                if (ns.childSizes == null)
                {
                    if (ns.expanded)
                    {
                        ns.initChildSizes();
                    }
                    else
                    {
                        return -1;
                    }
                }
                idx = ns.childSizes.getPosition(idx);
                position += idx + 1;
                node = parent;
                parent = node.Parent;
            }
            return position;
        }

        public int getRowFromNodeExpand(TreeTableNode node)
        {
            if (node.Parent != null)
            {
                TreeTableNode parent = node.Parent;
                int row = getRowFromNodeExpand(parent);
                int idx = parent.ChildIndexOf(node);
                NodeState ns = getOrCreateNodeState(parent);
                ns.Value = true;
                if (ns.childSizes == null)
                {
                    ns.initChildSizes();
                }
                return row + 1 + ns.childSizes.getPosition(idx);
            }
            else
            {
                return -1;
            }
        }

        public override TreeTableNode getNodeFromRow(int row)
        {
            NodeState ns = rootNodeState;
            for (; ; )
            {
                int idx;
                if (ns.childSizes == null)
                {
                    idx = Math.Min(ns.key.Children - 1, row);
                    row -= idx + 1;
                }
                else
                {
                    idx = ns.childSizes.getIndex(row);
                    row -= ns.childSizes.getPosition(idx) + 1;
                }
                if (row < 0)
                {
                    return ns.key.ChildAt(idx);
                }
                System.Diagnostics.Debug.Assert(ns.children[idx] != null);
                ns = ns.children[idx];
            }
        }

        public void collapseAll()
        {
            foreach (NodeState ns in this._states.Values)
            {
                if (ns != rootNodeState)
                {
                    ns.Value = false;
                }
            }
        }

        public bool isRowExpanded(int row)
        {
            checkRowIndex(row);
            TreeTableNode node = getNodeFromRow(row);
            NodeState ns = null;
            if (this._states.ContainsKey(node))
            {
                ns = this._states[node];
            }
            return (ns != null) && ns.expanded;
        }

        public void setRowExpanded(int row, bool expanded)
        {
            checkRowIndex(row);
            TreeTableNode node = getNodeFromRow(row);
            NodeState state = getOrCreateNodeState(node);
            state.Value = expanded;
        }

        public void setLeadRowExpanded(bool expanded)
        {
            TableSelectionManager sm = getSelectionManager();
            if (sm != null)
            {
                int row = sm.getLeadRow();
                if (row >= 0 && row < numRows)
                {
                    setRowExpanded(row, expanded);
                }
            }
        }

        protected NodeState getOrCreateNodeState(TreeTableNode node)
        {
            NodeState ns;
            if (!this._states.ContainsKey(node))
            {
                ns = createNodeState(node);
            }
            else
            {
                ns = this._states[node];
            }
            return ns;
        }

        protected NodeState createNodeState(TreeTableNode node)
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

        protected void expandedChanged(NodeState ns)
        {
            TreeTableNode node = ns.key;
            int count = ns.getChildRows();
            int size = ns.expanded ? count : 0;

            TreeTableNode parent = node.Parent;
            while (parent != null)
            {
                NodeState nsParent = this._states[parent];
                if (nsParent.childSizes == null)
                {
                    nsParent.initChildSizes();
                }

                int idx = nsParent.key.ChildIndexOf(node);
                nsParent.childSizes.setSize(idx, size + 1);
                size = nsParent.childSizes.getEndPosition();

                node = parent;
                parent = node.Parent;
            }

            numRows = computeNumRows();
            int row = getRowFromNode(ns.key);
            if (ns.expanded)
            {
                modelRowsInserted(row + 1, count);
            }
            else
            {
                modelRowsDeleted(row + 1, count);
            }
            modelRowsChanged(row, 1);

            if (ns.expanded)
            {
                ScrollPane scrollPane = ScrollPane.getContainingScrollPane(this);
                if (scrollPane != null)
                {
                    scrollPane.validateLayout();
                    int rowStart = getRowStartPosition(row);
                    int rowEnd = getRowEndPosition(row + count);
                    int height = rowEnd - rowStart;
                    scrollPane.scrollToAreaY(rowStart, height, rowHeight / 2);
                }
            }

            if (ns.expanded)
            {
                if (this.NodeExpanded != null)
                {
                    this.NodeExpanded.Invoke(this, new TreeTableNodeExpandedEventArgs(row, ns.key));
                }
            }
            else
            {
                if (this.NodeCollapsed != null)
                {
                    this.NodeCollapsed.Invoke(this, new TreeTableNodeCollapsedEventArgs(row, ns.key));
                }
            }
        }

        protected int computeNumRows()
        {
            return rootNodeState.childSizes.getEndPosition();
        }

        public override Object getCellData(int row, int column, TreeTableNode node)
        {
            if (node == null)
            {
                node = getNodeFromRow(row);
            }
            return node.DataAtColumn(column);
        }

        protected override CellRenderer getCellRenderer(int row, int col, TreeTableNode node)
        {
            if (node == null)
            {
                node = getNodeFromRow(row);
            }
            if (col == 0)
            {
                Object data = node.DataAtColumn(col);
                if (node.IsLeaf)
                {
                    leafRenderer.setCellData(row, col, data, node);
                    return leafRenderer;
                }
                NodeState nodeState = getOrCreateNodeState(node);
                nodeRenderer.setCellData(row, col, data, nodeState);
                return nodeRenderer;
            }
            return base.getCellRenderer(row, col, node);
        }

        public override Object getTooltipContentFromRow(int row, int column)
        {
            TreeTableNode node = getNodeFromRow(row);
            if (node != null)
            {
                return node.TooltipContentAtColumn(column);
            }
            return null;
        }

        private bool updateParentSizes(NodeState ns)
        {
            while (ns.expanded && ns.parent != null)
            {
                NodeState parent = ns.parent;
                int idx = parent.key.ChildIndexOf(ns.key);
                System.Diagnostics.Debug.Assert(parent.childSizes.size == parent.key.Children);
                parent.childSizes.setSize(idx, ns.getChildRows() + 1);
                ns = parent;
            }
            numRows = computeNumRows();
            return ns.parent == null;
        }

        protected void modelNodesAdded(TreeTableNode parent, int idx, int count)
        {
            if (!this._states.ContainsKey(parent))
            {
                return;
            }

            NodeState ns = this._states[parent];
            // if ns is null then this node has not yet been displayed
            if (ns != null)
            {
                if (ns.childSizes != null)
                {
                    System.Diagnostics.Debug.Assert(idx <= ns.childSizes.size);
                    ns.childSizes.insert(idx, count);
                    System.Diagnostics.Debug.Assert(ns.childSizes.size == parent.Children);
                }
                if (ns.children != null)
                {
                    NodeState[] newChilds = new NodeState[parent.Children];
                    Array.Copy(ns.children, 0, newChilds, 0, idx);
                    Array.Copy(ns.children, idx, newChilds, idx + count, ns.children.Length - idx);
                    ns.children = newChilds;
                }
                if (updateParentSizes(ns))
                {
                    int row = getRowFromNode(parent.ChildAt(idx));
                    System.Diagnostics.Debug.Assert(row < numRows);
                    modelRowsInserted(row, count);
                }
            }
        }

        protected void recursiveRemove(NodeState ns)
        {
            if (ns != null)
            {
                --nodeStateTableSize;
                this._states.Remove(ns.key);
                if (ns.children != null)
                {
                    foreach (NodeState nsChild in ns.children)
                    {
                        recursiveRemove(nsChild);
                    }
                }
            }
        }

        protected void modelNodesRemoved(TreeTableNode parent, int idx, int count)
        {
            NodeState ns = null;
            // if ns is null then this node has not yet been displayed
            if (this._states.ContainsKey(parent))
            {
                ns = this._states[parent];
            }
            if (ns != null)
            {
                int rowsBase = getRowFromNode(parent) + 1;
                int rowsStart = rowsBase + idx;
                int rowsEnd = rowsBase + idx + count;
                if (ns.childSizes != null)
                {
                    System.Diagnostics.Debug.Assert(ns.childSizes.size == parent.Children + count);
                    rowsStart = rowsBase + ns.childSizes.getPosition(idx);
                    rowsEnd = rowsBase + ns.childSizes.getPosition(idx + count);
                    ns.childSizes.remove(idx, count);
                    System.Diagnostics.Debug.Assert(ns.childSizes.size == parent.Children);
                }
                if (ns.children != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        recursiveRemove(ns.children[idx + i]);
                    }
                    int numChildren = parent.Children;
                    if (numChildren > 0)
                    {
                        NodeState[] newChilds = new NodeState[numChildren];
                        Array.Copy(ns.children, 0, newChilds, 0, idx);
                        Array.Copy(ns.children, idx + count, newChilds, idx, newChilds.Length - idx);
                        ns.children = newChilds;
                    }
                    else
                    {
                        ns.children = null;
                    }
                }
                if (updateParentSizes(ns))
                {
                    modelRowsDeleted(rowsStart, rowsEnd - rowsStart);
                }
            }
        }

        protected bool isVisible(NodeState ns)
        {
            while (ns.expanded && ns.parent != null)
            {
                ns = ns.parent;
            }
            return ns.expanded;
        }

        protected void modelNodesChanged(TreeTableNode parent, int idx, int count)
        {
            NodeState ns = null;
            // if ns is null then this node has not yet been displayed
            if (this._states.ContainsKey(parent))
            {
                ns = this._states[parent];
            }
            // if ns is null then this node has not yet been displayed
            if (ns != null && isVisible(ns))
            {
                int rowsBase = getRowFromNode(parent) + 1;
                int rowsStart = rowsBase + idx;
                int rowsEnd = rowsBase + idx + count;
                if (ns.childSizes != null)
                {
                    rowsStart = rowsBase + ns.childSizes.getPosition(idx);
                    rowsEnd = rowsBase + ns.childSizes.getPosition(idx + count);
                }
                modelRowsChanged(rowsStart, rowsEnd - rowsStart);
            }
        }

        public class NodeState : BooleanModel
        {
            protected internal NodeState parent;
            protected internal bool expanded;
            protected internal bool bHasNoChildren;
            protected internal SizeSequence childSizes;
            protected internal NodeState[] children;
            protected internal int level;
            protected internal TreeTableNode key;

            protected internal TreeTable treeTable;

            public bool Value
            {
                get
                {
                    return expanded;
                }
                set
                {
                    bool old = this.expanded;
                    if (this.expanded != value)
                    {
                        this.expanded = value;
                        this.treeTable.expandedChanged(this);
                        this.Changed.Invoke(this, new BooleanChangedEventArgs(old, this.expanded));
                    }
                }
            }

            public event EventHandler<BooleanChangedEventArgs> Changed;

            public NodeState(TreeTable treeTable, TreeTableNode key, NodeState parent)
            {
                this.treeTable = treeTable;
                this.key = key;
                this.parent = parent;
                this.level = (parent != null) ? parent.level + 1 : 0;

                if (parent != null)
                {
                    if (parent.children == null)
                    {
                        parent.children = new NodeState[parent.key.Children];
                    }
                    parent.children[parent.key.ChildIndexOf(key)] = this;
                }
            }

            protected internal void initChildSizes()
            {
                childSizes = new SizeSequence();
                childSizes.setDefaultValue(1);
                childSizes.initializeAll(key.Children);
            }

            protected internal int getChildRows()
            {
                if (childSizes != null)
                {
                    return childSizes.getEndPosition();
                }
                int childCount = key.Children;
                bHasNoChildren = childCount == 0;
                return childCount;
            }

            protected internal bool hasNoChildren()
            {
                return bHasNoChildren;
            }
        }

        static int getLevel(TreeTableNode node)
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
            protected int treeIndent;
            protected int level;
            protected Dimension treeButtonSize = new Dimension(5, 5);
            protected CellRenderer subRenderer;
            private TreeTable treeTable;

            public TreeLeafCellRenderer(TreeTable treeTable)
            {
                this.treeTable = treeTable;
                this.treeTable.setClip(true);
            }

            public void applyTheme(ThemeInfo themeInfo)
            {
                treeIndent = themeInfo.getParameter("treeIndent", 10);
                treeButtonSize = themeInfo.getParameterValue("treeButtonSize", true, typeof(Dimension), Dimension.ZERO);
            }

            public String getTheme()
            {
                return GetType().Name;
            }

            public void setCellData(int row, int column, Object data)
            {
                throw new InvalidOperationException("Don't call this method");
            }

            public void setCellData(int row, int column, Object data, TreeTableNode node)
            {
                level = getLevel(node);
                setSubRenderer(row, column, data);
            }

            protected int getIndentation()
            {
                return level * treeIndent + treeButtonSize.X;
            }

            protected virtual void setSubRenderer(int row, int column, Object colData)
            {
                subRenderer = this.treeTable.getCellRenderer(colData, column);
                if (subRenderer != null)
                {
                    subRenderer.setCellData(row, column, colData);
                }
            }

            public virtual int getColumnSpan()
            {
                return (subRenderer != null) ? subRenderer.getColumnSpan() : 1;
            }

            public int getPreferredHeight()
            {
                if (subRenderer != null)
                {
                    return Math.Max(treeButtonSize.Y, subRenderer.getPreferredHeight());
                }
                return treeButtonSize.Y;
            }

            public virtual Widget getCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                if (subRenderer != null)
                {
                    int indent = getIndentation();
                    Widget widget = subRenderer.getCellRenderWidget(
                            x + indent, y, Math.Max(0, width - indent), height, isSelected);
                    return widget;
                }
                return null;
            }

            public virtual Widget updateWidget(Widget existingWidget)
            {
                if (subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)subRenderer;
                    return subCreator.updateWidget(existingWidget);
                }
                return null;
            }

            public virtual void positionWidget(Widget widget, int x, int y, int w, int h)
            {
                if (subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)subRenderer;
                    int indent = level * treeIndent;
                    subCreator.positionWidget(widget, x + indent, y, Math.Max(0, w - indent), h);
                }
            }
        }

        class WidgetChain : Widget
        {
            protected internal ToggleButton expandButton;
            protected internal Widget userWidget;

            protected internal WidgetChain()
            {
                setTheme("");
                expandButton = new ToggleButton();
                expandButton.setTheme("treeButton");
                add(expandButton);
            }

            protected internal void setUserWidget(Widget userWidget)
            {
                if (this.userWidget != userWidget)
                {
                    if (this.userWidget != null)
                    {
                        removeChild(1);
                    }
                    this.userWidget = userWidget;
                    if (userWidget != null)
                    {
                        insertChild(userWidget, 1);
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

            public override Widget updateWidget(Widget existingWidget)
            {
                if (subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)subRenderer;
                    WidgetChain widgetChain = null;
                    if (existingWidget is WidgetChain)
                    {
                        widgetChain = (WidgetChain)existingWidget;
                    }
                    if (nodeState.hasNoChildren())
                    {
                        if (widgetChain != null)
                        {
                            existingWidget = null;
                        }
                        return subCreator.updateWidget(existingWidget);
                    }
                    if (widgetChain == null)
                    {
                        widgetChain = new WidgetChain();
                    }
                    widgetChain.expandButton.setModel(nodeState);
                    widgetChain.setUserWidget(subCreator.updateWidget(widgetChain.userWidget));
                    return widgetChain;
                }
                if (nodeState.hasNoChildren())
                {
                    return null;
                }
                ToggleButton tb = (ToggleButton)existingWidget;
                if (tb == null)
                {
                    tb = new ToggleButton();
                    tb.setTheme("treeButton");
                }
                tb.setModel(nodeState);
                return tb;
            }

            public override void positionWidget(Widget widget, int x, int y, int w, int h)
            {
                int indent = level * treeIndent;
                int availWidth = Math.Max(0, w - indent);
                int expandButtonWidth = Math.Min(availWidth, treeButtonSize.X);
                widget.setPosition(x + indent, y + (h - treeButtonSize.Y) / 2);
                if (subRenderer is CellWidgetCreator)
                {
                    CellWidgetCreator subCreator = (CellWidgetCreator)subRenderer;
                    WidgetChain widgetChain = (WidgetChain)widget;
                    ToggleButton expandButton = widgetChain.expandButton;
                    widgetChain.setSize(Math.Max(0, w - indent), h);
                    expandButton.setSize(expandButtonWidth, treeButtonSize.Y);
                    if (widgetChain.userWidget != null)
                    {
                        subCreator.positionWidget(widgetChain.userWidget,
                                expandButton.getRight(), y, widget.getWidth(), h);
                    }
                }
                else
                {
                    widget.setSize(expandButtonWidth, treeButtonSize.Y);
                }
            }

            public virtual void setCellData(int row, int column, Object data, NodeState nodeState)
            {
                System.Diagnostics.Debug.Assert(nodeState != null);
                this.nodeState = nodeState;
                setSubRenderer(row, column, data);
                level = nodeState.level;
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
