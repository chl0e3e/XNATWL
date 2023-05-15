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
using XNATWL.Utils;
using static XNATWL.Utils.Logger;

namespace XNATWL
{
    public class PropertySheet<T> : TreeTable
    {
        public interface PropertyEditor
        {
            Widget GetWidget();
            void ValueChanged();
            void PreDestroy();
            void SetSelected(bool selected);

            /**
             * Can be used to position the widget in a cell.
             * <p>If this method returns false, the table will position the widget itself.</p>
             *
             * <p>This method is responsible to call setPosition and setSize on the
             * widget or return false.</p>
             *
             * @param x the left edge of the cell
             * @param y the top edge of the cell
             * @param width the width of the cell
             * @param height the height of the cell
             * 
             * @return true if the position was changed by this method.
             */
            bool PositionWidget(int x, int y, int width, int height);
        }

        public interface PropertyEditorFactory
        {
            PropertyEditor CreateEditor(XNATWL.Model.Property property);
        }

        private SimplePropertyList _rootList;
        private PropertyListCellRenderer _subListRenderer;
        private CellRenderer _editorRenderer;
        private TypeMapping _factories;

        public PropertySheet() : this(new Model())
        {

        }

        private PropertySheet(Model model) : base(model)
        {
            this._rootList = new SimplePropertyList("<root>");
            this._subListRenderer = new PropertyListCellRenderer(this);
            this._editorRenderer = new EditorRenderer();
            this._factories = new TypeMapping();
            TreeGenerator treeGenerator = new TreeGenerator(this, this._rootList, model);
            _rootList.Changed += (sender, e) =>
            {
                treeGenerator.Run();
            };
            RegisterPropertyEditorFactory<string>(typeof(String), new StringEditorFactory());
        }

        public SimplePropertyList GetPropertyList()
        {
            return _rootList;
        }

        public void RegisterPropertyEditorFactory<T>(Type clazz, PropertyEditorFactory factory)
        {
            if (clazz == null)
            {
                throw new NullReferenceException("clazz");
            }
            if (factory == null)
            {
                throw new NullReferenceException("factory");
            }
            _factories.SetByType(clazz, factory);
        }

        public override void SetModel(TreeTableModel model)
        {
            if (model is Model)
            {
                base.SetModel(model);
            }
            else
            {
                throw new InvalidOperationException("Do not call this method");
            }
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemePropertiesSheet(themeInfo);
        }

        protected void ApplyThemePropertiesSheet(ThemeInfo themeInfo)
        {
            ApplyCellRendererTheme(_subListRenderer);
            ApplyCellRendererTheme(_editorRenderer);
        }

        protected override CellRenderer GetCellRenderer(int row, int col, TreeTableNode node)
        {
            if (node == null)
            {
                node = GetNodeFromRow(row);
            }
            if (node is ListNode)
            {
                if (col == 0)
                {
                    PropertyListCellRenderer cr = _subListRenderer;
                    NodeState nodeState = GetOrCreateNodeState(node);
                    cr.SetCellData(row, col, node.DataAtColumn(col), nodeState);
                    return cr;
                }
                else
                {
                    return null;
                }
            }
            else if (col == 0)
            {
                return base.GetCellRenderer(row, col, node);
            }
            else
            {
                CellRenderer cr = _editorRenderer;
                cr.SetCellData(row, col, node.DataAtColumn(col));
                return cr;
            }
        }

        TreeTableNode CreateNode(TreeTableNode parent, XNATWL.Model.Property property)
        {
            if (property is PropertyList)
            {
                return new ListNode(this, parent, property);
            }
            else
            {
                Type type = property.Type;
                PropertyEditorFactory factory = (PropertyEditorFactory)_factories.GetByType(type);
                if (factory != null)
                {
                    PropertyEditor editor = factory.CreateEditor(property);
                    if (editor != null)
                    {
                        return new LeafNode(this, parent, property, editor);
                    }
                }
                else
                {
                    Logger.GetLogger(typeof(PropertySheet<T>)).Log(Level.WARNING, "No property editor factory for type " + type.FullName);
                }
                return null;
            }
        }

        interface PSTreeTableNode : TreeTableNode
        {
            void AddChild(TreeTableNode parent);
            void RemoveAllChildren();
        }

        abstract class PropertyNode : AbstractTreeTableNode, PSTreeTableNode
        {
            protected XNATWL.Model.Property _property;
            protected PropertySheet<T> _propertySheet;

            public PropertyNode(PropertySheet<T> propertySheet, TreeTableNode parent, XNATWL.Model.Property property) : base(parent)
            {
                this._propertySheet = propertySheet;
                this._property = property;

                property.Changed += Property_Changed;
            }

            private void Property_Changed(object sender, PropertyChangedEventArgs e)
            {
                this.Run();
            }

            public abstract void Run();

            protected internal virtual void RemoveCallback()
            {
                _property.Changed -= Property_Changed;
            }

            public override void RemoveAllChildren()
            {
                base.RemoveAllChildren();
            }

            public void AddChild(TreeTableNode parent)
            {
                InsertChild(parent, base.Children);
            }
        }

        class TreeGenerator
        {
            private PropertyList _list;
            private PSTreeTableNode _parent;
            private PropertySheet<T> _propertySheet;

            public TreeGenerator(PropertySheet<T> propertySheet, PropertyList list, PSTreeTableNode parent)
            {
                this._propertySheet = propertySheet;
                this._list = list;
                this._parent = parent;
            }

            public void Run()
            {
                _parent.RemoveAllChildren();
                AddSubProperties();
            }

            protected internal void RemoveChildCallbacks(PSTreeTableNode parent)
            {
                for (int i = 0, n = parent.Children; i < n; ++i)
                {
                    ((PropertyNode)parent.ChildAt(i)).RemoveCallback();
                }
            }

            protected internal void AddSubProperties()
            {
                for (int i = 0; i < _list.Count; ++i)
                {
                    TreeTableNode node = this._propertySheet.CreateNode(_parent, (XNATWL.Model.Property)_list.PropertyAt(i));
                    if (node != null)
                    {
                        _parent.AddChild(node);
                    }
                }
            }
        }

        class LeafNode : PropertyNode
        {
            private PropertyEditor _editor;

            public LeafNode(PropertySheet<T> propertySheet, TreeTableNode parent, XNATWL.Model.Property property, PropertyEditor editor) : base(propertySheet, parent, property)
            {
                this._editor = editor;
                this.IsLeaf = true;
            }

            public override Object DataAtColumn(int column)
            {
                switch (column)
                {
                    case 0: return _property.Name;
                    case 1: return _editor;
                    default: return "???";
                }
            }

            public override void Run()
            {
                _editor.ValueChanged();
                this.FireNodeChanged();
            }
        }

        class ListNode : PropertyNode
        {
            protected TreeGenerator _treeGenerator;

            public ListNode(PropertySheet<T> propertySheet, TreeTableNode parent, XNATWL.Model.Property property) : base(propertySheet, parent, property)
            {
                this._treeGenerator = new TreeGenerator(propertySheet, (PropertyList)property.Value, this);
                _treeGenerator.Run();
            }

            public override Object DataAtColumn(int column)
            {
                return _property.Name;
            }

            public override void Run()
            {
                _treeGenerator.Run();
            }

            protected internal override void RemoveCallback()
            {
                base.RemoveCallback();
                _treeGenerator.RemoveChildCallbacks(this);
            }
        }

        class PropertyListCellRenderer : TreeNodeCellRenderer
        {
            private Widget _bgRenderer;
            private Label _textRenderer;
            private PropertySheet<T> propertySheet;

            public PropertyListCellRenderer(PropertySheet<T> propertySheet) : base(propertySheet)
            {
                this.propertySheet = propertySheet;
                _bgRenderer = new Widget();
                _textRenderer = new Label(_bgRenderer.GetAnimationState());
                _textRenderer.SetAutoSize(false);
                _bgRenderer.Add(_textRenderer);
                _bgRenderer.SetTheme(GetTheme());
            }
            public override int GetColumnSpan()
            {
                return 2;
            }
            public override Widget GetCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                _bgRenderer.SetPosition(x, y);
                _bgRenderer.SetSize(width, height);
                int indent = GetIndentation();
                _textRenderer.SetPosition(x + indent, y);
                _textRenderer.SetSize(Math.Max(0, width - indent), height);
                _bgRenderer.GetAnimationState().SetAnimationState(STATE_SELECTED, isSelected);
                return _bgRenderer;
            }
            public override void SetCellData(int row, int column, Object data, NodeState nodeState)
            {
                base.SetCellData(row, column, data, nodeState);
                _textRenderer.SetText((String)data);
            }
            protected override void SetSubRenderer(int row, int column, Object colData)
            {
            }
        }

        class EditorRenderer : CellRenderer, TreeTable.CellWidgetCreator
        {
            private PropertyEditor _editor;

            public void ApplyTheme(ThemeInfo themeInfo)
            {
            }
            public Widget GetCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                _editor.SetSelected(isSelected);
                return null;
            }
            public int GetColumnSpan()
            {
                return 1;
            }
            public int GetPreferredHeight()
            {
                return _editor.GetWidget().GetPreferredHeight();
            }
            public String GetTheme()
            {
                return "PropertyEditorCellRender";
            }
            public void SetCellData(int row, int column, Object data)
            {
                _editor = (PropertyEditor)data;
            }
            public Widget UpdateWidget(Widget existingWidget)
            {
                return _editor.GetWidget();
            }
            public void PositionWidget(Widget widget, int x, int y, int w, int h)
            {
                if (!_editor.PositionWidget(x, y, w, h))
                {
                    widget.SetPosition(x, y);
                    widget.SetSize(w, h);
                }
            }
        }

        class Model : AbstractTreeTableModel, PSTreeTableNode
        {
            public override int Columns
            {
                get
                {
                    return 2;
                }
            }

            public override event EventHandler<ColumnsChangedEventArgs> ColumnInserted;
            public override event EventHandler<ColumnsChangedEventArgs> ColumnDeleted;
            public override event EventHandler<ColumnHeaderChangedEventArgs> ColumnHeaderChanged;

            public override void RemoveAllChildren()
            {
                base.RemoveAllChildren();
            }

            public void AddChild(TreeTableNode parent)
            {
                this.InsertChildAt(parent, this.Children);
            }

            public override string ColumnHeaderTextFor(int column)
            {
                switch (column)
                {
                    case 0: return "Name";
                    case 1: return "Value";
                    default: return "???";
                }
            }
        }

        class StringEditor : PropertyEditor
        {
            private EditField _editField;
            private Property<String> _property;

            public StringEditor(Property<String> property)
            {
                this._property = property;
                this._editField = new EditField();
                _editField.Callback += EditField_Callback;
                ResetValue();
            }

            private void EditField_Callback(object sender, EditFieldCallbackEventArgs e)
            {
                if (e.Key == Event.KEY_ESCAPE)
                {
                    ResetValue();
                }
                else if (!_property.IsReadOnly)
                {
                    try
                    {
                        _property.Value = _editField.GetText();
                        _editField.SetErrorMessage(null);
                    }
                    catch (ArgumentException ex)
                    {
                        _editField.SetErrorMessage(ex.Message);
                    }
                }
            }

            public Widget GetWidget()
            {
                return _editField;
            }
            public void ValueChanged()
            {
                ResetValue();
            }
            public void PreDestroy()
            {
                _editField.Callback -= EditField_Callback;
            }
            public void SetSelected(bool selected)
            {
            }
            private void ResetValue()
            {
                _editField.SetText((string)_property.Value);
                _editField.SetErrorMessage(null);
                _editField.SetReadOnly(_property.IsReadOnly);
            }
            public bool PositionWidget(int x, int y, int width, int height)
            {
                return false;
            }
        }

        class StringEditorFactory : PropertyEditorFactory
        {
            public PropertyEditor CreateEditor(XNATWL.Model.Property property)
            {
                if (property is Property<string>)
                {
                    return new StringEditor((Property<string>)property);
                }

                throw new Exception("StringEditorFactory given property that isn't of string generic");
            }
        }

        public class ComboBoxEditor<T> : PropertyEditor
        {
            protected ComboBox<T> _comboBox;
            protected Property<T> _property;
            protected ListModel<T> _model;

            public ComboBoxEditor(Property<T> property, ListModel<T> model)
            {
                this._property = property;
                this._comboBox = new ComboBox<T>(model);
                this._model = model;
                _comboBox.SelectionChanged += ComboBox_SelectionChanged;
            }

            private void ComboBox_SelectionChanged(object sender, ComboBoxSelectionChangedEventArgs e)
            {
                if (_property.IsReadOnly)
                {
                    ResetValue();
                }
                else
                {
                    int idx = _comboBox.GetSelected();
                    if (idx >= 0)
                    {
                        _property.Value = _model.EntryAt(idx);
                    }
                }
            }

            public Widget GetWidget()
            {
                return _comboBox;
            }

            public void ValueChanged()
            {
                ResetValue();
            }

            public void PreDestroy()
            {
                _comboBox.SelectionChanged -= ComboBox_SelectionChanged;
            }

            public void SetSelected(bool selected)
            {
            }

            public void Run()
            {
                if (_property.IsReadOnly)
                {
                    ResetValue();
                }
                else
                {
                    int idx = _comboBox.GetSelected();
                    if (idx >= 0)
                    {
                        _property.Value = (_model.EntryAt(idx));
                    }
                }
            }

            protected void ResetValue()
            {
                _comboBox.SetSelected(FindEntry(_property.ValueCast));
            }

            protected int FindEntry(T value)
            {
                for (int i = 0, n = _model.Entries; i < n; i++)
                {
                    if (_model.EntryAt(i).Equals(value))
                    {
                        return i;
                    }
                }
                return -1;
            }

            public bool PositionWidget(int x, int y, int width, int height)
            {
                return false;
            }
        }

        public class ComboBoxEditorFactory : PropertyEditorFactory
        {
            private ModelForwarder _modelForwarder;
            public ComboBoxEditorFactory(ListModel<T> model)
            {
                this._modelForwarder = new ModelForwarder(model);
            }

            public ListModel<T> GetModel()
            {
                return _modelForwarder.GetModel();
            }

            public void SetModel(ListModel<T> model)
            {
                _modelForwarder.SetModel(model);
            }

            public PropertyEditor CreateEditor(XNATWL.Model.Property property)
            {
                return new ComboBoxEditor<T>((Property<T>)property, _modelForwarder);
            }

            class ModelForwarder : AbstractListModel<T>
            {
                private ListModel<T> _model;

                public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
                public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
                public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
                public override event EventHandler<ListAllChangedEventArgs> AllChanged;

                public override int Entries
                {
                    get
                    {
                        return _model.Entries;
                    }
                }

                public ModelForwarder(ListModel<T> model)
                {
                    SetModel(model);
                }

                public ListModel<T> GetModel()
                {
                    return _model;
                }

                public void SetModel(ListModel<T> model)
                {
                    if (this._model != null)
                    {
                        this._model.EntriesChanged -= Model_EntriesChanged;
                        this._model.EntriesDeleted -= Model_EntriesDeleted;
                        this._model.EntriesInserted -= Model_EntriesInserted;
                        this._model.AllChanged -= Model_AllChanged;
                    }
                    this._model = model;
                    this._model.EntriesChanged += Model_EntriesChanged;
                    this._model.EntriesDeleted += Model_EntriesDeleted;
                    this._model.EntriesInserted += Model_EntriesInserted;
                    this._model.AllChanged += Model_AllChanged;
                    this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
                }

                private void Model_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
                {
                    this.EntriesInserted.Invoke(this, e);
                }

                private void Model_AllChanged(object sender, ListAllChangedEventArgs e)
                {
                    this.AllChanged.Invoke(this, e);
                }

                private void Model_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
                {
                    this.EntriesChanged.Invoke(this, e);
                }

                private void Model_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
                {
                    this.EntriesDeleted.Invoke(this, e);
                }

                public override T EntryAt(int index)
                {
                    return _model.EntryAt(index);
                }

                public override bool EntryMatchesPrefix(int index, string prefix)
                {
                    return _model.EntryMatchesPrefix(index, prefix);
                }

                public override object EntryTooltipAt(int index)
                {
                    return _model.EntryTooltipAt(index);
                }
            }
        }
    }
}
