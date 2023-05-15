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
using XNATWL.Model;

namespace XNATWL
{

    public class TreeComboBox : ComboBoxBase
    {
        public interface PathResolver
        {
            /**
             * Tries to resolve the given string to a node
             *
             * @param model the tree model
             * @param path the path to resolve
             * @return A node - MUST NOT BE NULL
             * @throws IllegalArgumentException when the path can't be resolved, the message is displayed
             */
            TreeTableNode resolvePath(TreeTableModel model, String path);
        }

        public event EventHandler<TreeComboBoxSelectedNodeChanged> SelectedNodeChanged;

        private static String DEFAULT_POPUP_THEME = "treecomboboxPopup";

        TableSingleSelectionModel selectionModel;
        TreePathDisplay display;
        TreeTable table;

        private TreeTableModel model;
        private PathResolver pathResolver;
        private bool suppressCallback;

        bool suppressTreeSelectionUpdating;

        class TreeComboBoxPathDisplay : TreePathDisplay
        {
            private TreeComboBox treeComboBox;
            public TreeComboBoxPathDisplay(TreeComboBox treeComboBox)
            {
                this.treeComboBox = treeComboBox;
            }
            public override bool resolvePath(string path)
            {
                return this.treeComboBox.resolvePath(path);
            }
        }

        class TreeComboBoxTableSelectionManager : TableRowSelectionManager
        {
            private TreeComboBox treeComboBox;
            public TreeComboBoxTableSelectionManager(TreeComboBox treeComboBox)
            {
                this.treeComboBox = treeComboBox;
            }
            protected override bool handleMouseClick(int row, int column, bool isShift, bool isCtrl)
            {
                if (!isShift && !isCtrl && row >= 0 && row < getNumRows())
                {
                    this.treeComboBox.popup.closePopup();
                    return true;
                }
                return base.handleMouseClick(row, column, isShift, isCtrl);
            }
        }

        public TreeComboBox()
        {
            selectionModel = new TableSingleSelectionModel();
            display = new TreeComboBoxPathDisplay(this);
            display.setTheme("display");
            table = new TreeTable();
            table.setSelectionManager(new TreeComboBoxTableSelectionManager(this));
            display.PathElementClicked += Display_PathElementClicked;
            selectionModel.SelectionChanged += SelectionModel_SelectionChanged;

            ScrollPane scrollPane = new ScrollPane(table);
            scrollPane.setFixed(ScrollPane.Fixed.HORIZONTAL);

            add(display);
            popup.setTheme(DEFAULT_POPUP_THEME);
            popup.add(scrollPane);
        }

        private void SelectionModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int row = selectionModel.FirstSelected;
            if (row >= 0)
            {
                suppressTreeSelectionUpdating = true;
                try
                {
                    nodeChanged(table.getNodeFromRow(row));
                }
                finally
                {
                    suppressTreeSelectionUpdating = false;
                }
            }
        }

        private void Display_PathElementClicked(object sender, TreePathElementClickedEventArgs e)
        {
            fireSelectedNodeChanged(e.Node, e.ChildNode);
        }

        public TreeComboBox(TreeTableModel model) : this()
        {
            setModel(model);
        }

        public TreeTableModel getModel()
        {
            return model;
        }

        public void setModel(TreeTableModel model)
        {
            if (this.model != model)
            {
                this.model = model;
                table.setModel(model);
                display.setCurrentNode(model);
            }
        }

        public void setCurrentNode(TreeTableNode node)
        {
            if (node == null)
            {
                throw new NullReferenceException("node");
            }
            display.setCurrentNode(node);
            if (popup.isOpen())
            {
                tableSelectToCurrentNode();
            }
        }

        public TreeTableNode getCurrentNode()
        {
            return display.getCurrentNode();
        }

        public void setSeparator(String separator)
        {
            display.setSeparator(separator);
        }

        public String getSeparator()
        {
            return display.getSeparator();
        }

        public PathResolver getPathResolver()
        {
            return pathResolver;
        }

        public void setPathResolver(PathResolver pathResolver)
        {
            this.pathResolver = pathResolver;
            display.setAllowEdit(pathResolver != null);
        }

        public TreeTable getTreeTable()
        {
            return table;
        }

        public EditField getEditField()
        {
            return display.getEditField();
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyTreeComboboxPopupThemeName(themeInfo);
        }

        protected void applyTreeComboboxPopupThemeName(ThemeInfo themeInfo)
        {
            popup.setTheme(themeInfo.GetParameter("popupThemeName", DEFAULT_POPUP_THEME));
        }

        protected override Widget getLabel()
        {
            return display;
        }

        void fireSelectedNodeChanged(TreeTableNode node, TreeTableNode child)
        {
            if (this.SelectedNodeChanged != null)
            {
                this.SelectedNodeChanged.Invoke(this, new TreeComboBoxSelectedNodeChanged(node, child));
            }
        }

        bool resolvePath(String path)
        {
            if (pathResolver != null)
            {
                try
                {
                    TreeTableNode node = pathResolver.resolvePath(model, path);
                    System.Diagnostics.Debug.Assert(node != null);
                    nodeChanged(node);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    display.setEditErrorMessage(ex.Message);
                }
            }
            return false;
        }

        void nodeChanged(TreeTableNode node)
        {
            TreeTableNode oldNode = display.getCurrentNode();
            display.setCurrentNode(node);
            if (!suppressCallback)
            {
                fireSelectedNodeChanged(node, getChildOf(node, oldNode));
            }
        }

        private TreeTableNode getChildOf(TreeTableNode parent, TreeTableNode node)
        {
            while (node != null && node != parent)
            {
                node = node.Parent;
            }
            return node;
        }

        private void tableSelectToCurrentNode()
        {
            if (!suppressTreeSelectionUpdating)
            {
                table.collapseAll();
                int idx = table.getRowFromNodeExpand(display.getCurrentNode());
                suppressCallback = true;
                try
                {
                    selectionModel.SetSelection(idx, idx);
                }
                finally
                {
                    suppressCallback = false;
                }
                table.scrollToRow(Math.Max(0, idx));
            }
        }

        protected override bool openPopup()
        {
            if (base.openPopup())
            {
                popup.validateLayout();
                tableSelectToCurrentNode();
                return true;
            }
            return false;
        }

    }

    public class TreeComboBoxSelectedNodeChanged : EventArgs
    {
        public TreeTableNode Node;
        public TreeTableNode PreviousChildNode;
        //TreeTableNode node, TreeTableNode previousChildNode
        public TreeComboBoxSelectedNodeChanged(TreeTableNode node, TreeTableNode previousChildNode)
        {
            this.Node = node;
            this.PreviousChildNode = previousChildNode;
        }
    }
}
