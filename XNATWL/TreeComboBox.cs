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
            TreeTableNode ResolvePath(TreeTableModel model, String path);
        }

        public event EventHandler<TreeComboBoxSelectedNodeChanged> SelectedNodeChanged;

        private static String DEFAULT_POPUP_THEME = "treecomboboxPopup";

        TableSingleSelectionModel _selectionModel;
        TreePathDisplay _display;
        TreeTable _table;

        private TreeTableModel _model;
        private PathResolver _pathResolver;
        private bool _suppressCallback;

        bool _suppressTreeSelectionUpdating;

        class TreeComboBoxPathDisplay : TreePathDisplay
        {
            private TreeComboBox _treeComboBox;
            public TreeComboBoxPathDisplay(TreeComboBox treeComboBox)
            {
                this._treeComboBox = treeComboBox;
            }
            public override bool ResolvePath(string path)
            {
                return this._treeComboBox.ResolvePath(path);
            }
        }

        class TreeComboBoxTableSelectionManager : TableRowSelectionManager
        {
            private TreeComboBox _treeComboBox;
            public TreeComboBoxTableSelectionManager(TreeComboBox treeComboBox)
            {
                this._treeComboBox = treeComboBox;
            }
            protected override bool HandleMouseClick(int row, int column, bool isShift, bool isCtrl)
            {
                if (!isShift && !isCtrl && row >= 0 && row < GetNumRows())
                {
                    this._treeComboBox._popup.ClosePopup();
                    return true;
                }
                return base.HandleMouseClick(row, column, isShift, isCtrl);
            }
        }

        public TreeComboBox()
        {
            _selectionModel = new TableSingleSelectionModel();
            _display = new TreeComboBoxPathDisplay(this);
            _display.SetTheme("display");
            _table = new TreeTable();
            _table.SetSelectionManager(new TreeComboBoxTableSelectionManager(this));
            _display.PathElementClicked += Display_PathElementClicked;
            _selectionModel.SelectionChanged += SelectionModel_SelectionChanged;

            ScrollPane scrollPane = new ScrollPane(_table);
            scrollPane.SetFixed(ScrollPane.Fixed.HORIZONTAL);

            Add(_display);
            _popup.SetTheme(DEFAULT_POPUP_THEME);
            _popup.Add(scrollPane);
        }

        private void SelectionModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int row = _selectionModel.FirstSelected;
            if (row >= 0)
            {
                _suppressTreeSelectionUpdating = true;
                try
                {
                    NodeChanged(_table.GetNodeFromRow(row));
                }
                finally
                {
                    _suppressTreeSelectionUpdating = false;
                }
            }
        }

        private void Display_PathElementClicked(object sender, TreePathElementClickedEventArgs e)
        {
            FireSelectedNodeChanged(e.Node, e.ChildNode);
        }

        public TreeComboBox(TreeTableModel model) : this()
        {
            SetModel(model);
        }

        public TreeTableModel GetModel()
        {
            return _model;
        }

        public void SetModel(TreeTableModel model)
        {
            if (this._model != model)
            {
                this._model = model;
                _table.SetModel(model);
                _display.SetCurrentNode(model);
            }
        }

        public void SetCurrentNode(TreeTableNode node)
        {
            if (node == null)
            {
                throw new NullReferenceException("node");
            }
            _display.SetCurrentNode(node);
            if (_popup.IsOpen())
            {
                TableSelectToCurrentNode();
            }
        }

        public TreeTableNode GetCurrentNode()
        {
            return _display.GetCurrentNode();
        }

        public void SetSeparator(String separator)
        {
            _display.SetSeparator(separator);
        }

        public String GetSeparator()
        {
            return _display.GetSeparator();
        }

        public PathResolver GetPathResolver()
        {
            return _pathResolver;
        }

        public void SetPathResolver(PathResolver pathResolver)
        {
            this._pathResolver = pathResolver;
            _display.SetAllowEdit(pathResolver != null);
        }

        public TreeTable GetTreeTable()
        {
            return _table;
        }

        public EditField GetEditField()
        {
            return _display.GetEditField();
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyTreeComboboxPopupThemeName(themeInfo);
        }

        protected void ApplyTreeComboboxPopupThemeName(ThemeInfo themeInfo)
        {
            _popup.SetTheme(themeInfo.GetParameter("popupThemeName", DEFAULT_POPUP_THEME));
        }

        protected override Widget GetLabel()
        {
            return _display;
        }

        void FireSelectedNodeChanged(TreeTableNode node, TreeTableNode child)
        {
            if (this.SelectedNodeChanged != null)
            {
                this.SelectedNodeChanged.Invoke(this, new TreeComboBoxSelectedNodeChanged(node, child));
            }
        }

        bool ResolvePath(String path)
        {
            if (_pathResolver != null)
            {
                try
                {
                    TreeTableNode node = _pathResolver.ResolvePath(_model, path);
                    System.Diagnostics.Debug.Assert(node != null);
                    NodeChanged(node);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    _display.SetEditErrorMessage(ex.Message);
                }
            }
            return false;
        }

        void NodeChanged(TreeTableNode node)
        {
            TreeTableNode oldNode = _display.GetCurrentNode();
            _display.SetCurrentNode(node);
            if (!_suppressCallback)
            {
                FireSelectedNodeChanged(node, GetChildOf(node, oldNode));
            }
        }

        private TreeTableNode GetChildOf(TreeTableNode parent, TreeTableNode node)
        {
            while (node != null && node != parent)
            {
                node = node.Parent;
            }
            return node;
        }

        private void TableSelectToCurrentNode()
        {
            if (!_suppressTreeSelectionUpdating)
            {
                _table.CollapseAll();
                int idx = _table.GetRowFromNodeExpand(_display.GetCurrentNode());
                _suppressCallback = true;
                try
                {
                    _selectionModel.SetSelection(idx, idx);
                }
                finally
                {
                    _suppressCallback = false;
                }
                _table.ScrollToRow(Math.Max(0, idx));
            }
        }

        protected override bool OpenPopup()
        {
            if (base.OpenPopup())
            {
                _popup.ValidateLayout();
                TableSelectToCurrentNode();
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
