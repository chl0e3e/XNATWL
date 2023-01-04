using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Utils.Logger;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class PropertySheet<T> : TreeTable
    {
        public interface PropertyEditor
        {
            Widget getWidget();
            void valueChanged();
            void preDestroy();
            void setSelected(bool selected);

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
            bool positionWidget(int x, int y, int width, int height);
        }

        public interface PropertyEditorFactory
        {
            PropertyEditor createEditor(Property<T> property);
        }

        private SimplePropertyList<T> rootList;
        private PropertyListCellRenderer subListRenderer;
        private CellRenderer editorRenderer;
        private TypeMapping factories;

        public PropertySheet() : this(new Model())
        {

        }

        private PropertySheet(Model model) : base(model)
        {
            this.rootList = new SimplePropertyList<T>("<root>");
            this.subListRenderer = new PropertyListCellRenderer(this);
            this.editorRenderer = new EditorRenderer();
            this.factories = new TypeMapping();
            TreeGenerator treeGenerator = new TreeGenerator(this, this.rootList, model);
            rootList.Changed += (sender, e) =>
            {
                treeGenerator.run();
            };
            registerPropertyEditorFactory<string>(typeof(String), new StringEditorFactory());
        }

        public SimplePropertyList<T> getPropertyList()
        {
            return rootList;
        }

        public void registerPropertyEditorFactory<T>(Type clazz, PropertyEditorFactory factory)
        {
            if (clazz == null)
            {
                throw new NullReferenceException("clazz");
            }
            if (factory == null)
            {
                throw new NullReferenceException("factory");
            }
            factories.SetByType(clazz, factory);
        }

        public override void setModel(TreeTableModel model)
        {
            if (model is Model)
            {
                base.setModel(model);
            }
            else
            {
                throw new InvalidOperationException("Do not call this method");
            }
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemePropertiesSheet(themeInfo);
        }

        protected void applyThemePropertiesSheet(ThemeInfo themeInfo)
        {
            applyCellRendererTheme(subListRenderer);
            applyCellRendererTheme(editorRenderer);
        }

        protected override CellRenderer getCellRenderer(int row, int col, TreeTableNode node)
        {
            if (node == null)
            {
                node = getNodeFromRow(row);
            }
            if (node is ListNode)
            {
                if (col == 0)
                {
                    PropertyListCellRenderer cr = subListRenderer;
                    NodeState nodeState = getOrCreateNodeState(node);
                    cr.setCellData(row, col, node.DataAtColumn(col), nodeState);
                    return cr;
                }
                else
                {
                    return null;
                }
            }
            else if (col == 0)
            {
                return base.getCellRenderer(row, col, node);
            }
            else
            {
                CellRenderer cr = editorRenderer;
                cr.setCellData(row, col, node.DataAtColumn(col));
                return cr;
            }
        }

        TreeTableNode createNode(TreeTableNode parent, Property<T> property)
        {
            if (property.GetType() == typeof(PropertyList<T>))
            {
                return new ListNode(this, parent, property);
            }
            else
            {
                Type type = property.Type;
                PropertyEditorFactory factory = (PropertyEditorFactory) factories.GetByType(type);
                if (factory != null)
                {
                    PropertyEditor editor = factory.createEditor(property);
                    if (editor != null)
                    {
                        return new LeafNode(this, parent, property, editor);
                    }
                }
                else
                {
                    Logger.GetLogger(typeof(PropertySheet<T>)).log(Level.WARNING, "No property editor factory for type " + type.FullName);
                }
                return null;
            }
        }

        interface PSTreeTableNode : TreeTableNode
        {
            void addChild(TreeTableNode parent);
            void RemoveAllChildren();
        }

        abstract class PropertyNode : AbstractTreeTableNode, PSTreeTableNode
        {
            protected Property<T> property;
            protected PropertySheet<T> propertySheet;

            public PropertyNode(PropertySheet<T> propertySheet, TreeTableNode parent, Property<T> property) : base(parent)
            {
                this.propertySheet = propertySheet;
                this.property = property;
                property.Changed += Property_Changed;
            }

            private void Property_Changed(object sender, PropertyChangedEventArgs<T> e)
            {
                this.run();
            }

            public abstract void run();

            protected internal virtual void removeCallback()
            {
                property.Changed -= Property_Changed;
            }
            public override void RemoveAllChildren()
            {
                base.RemoveAllChildren();
            }
            public void addChild(TreeTableNode parent)
            {
                InsertChild(parent, base.Children);
            }
        }

        class TreeGenerator
        {
            private PropertyList<T> list;
            private PSTreeTableNode parent;
            private PropertySheet<T> propertySheet;

            public TreeGenerator(PropertySheet<T> propertySheet, PropertyList<T> list, PSTreeTableNode parent)
            {
                this.propertySheet = propertySheet;
                this.list = list;
                this.parent = parent;
            }
            public void run()
            {
                parent.RemoveAllChildren();
                addSubProperties();
            }
            protected internal void removeChildCallbacks(PSTreeTableNode parent)
            {
                for (int i = 0, n = parent.Children; i < n; ++i)
                {
                    ((PropertyNode)parent.ChildAt(i)).removeCallback();
                }
            }
            protected internal void addSubProperties()
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    TreeTableNode node = this.propertySheet.createNode(parent, list.PropertyAt(i));
                    if (node != null)
                    {
                        parent.addChild(node);
                    }
                }
            }
        }

        class LeafNode : PropertyNode
        {
            private PropertyEditor editor;

            public LeafNode(PropertySheet<T> propertySheet, TreeTableNode parent, Property<T> property, PropertyEditor editor) : base(propertySheet, parent, property)
            {
                this.editor = editor;
                this.IsLeaf = true;
            }
            public Object getData(int column)
            {
                switch (column)
                {
                    case 0: return property.Name;
                    case 1: return editor;
                    default: return "???";
                }
            }
            public override void run()
            {
                editor.valueChanged();
                this.FireNodeChanged();
            }
        }

        class ListNode : PropertyNode
        {
            protected TreeGenerator treeGenerator;

            public ListNode(PropertySheet<T> propertySheet, TreeTableNode parent, Property<T> property) : base(propertySheet, parent, property)
            {
                this.treeGenerator = new TreeGenerator(propertySheet, (PropertyList<T>)property.Value, this);
                treeGenerator.run();
            }
            public Object getData(int column)
            {
                return property.Name;
            }
            public override void run()
            {
                treeGenerator.run();
            }
            protected internal override void removeCallback()
            {
                base.removeCallback();
                treeGenerator.removeChildCallbacks(this);
            }
        }

        class PropertyListCellRenderer : TreeNodeCellRenderer
        {
            private Widget bgRenderer;
            private Label textRenderer;
            private PropertySheet<T> propertySheet;

            public PropertyListCellRenderer(PropertySheet<T> propertySheet) : base(propertySheet)
            {
                this.propertySheet = propertySheet;
                bgRenderer = new Widget();
                textRenderer = new Label(bgRenderer.getAnimationState());
                textRenderer.setAutoSize(false);
                bgRenderer.add(textRenderer);
                bgRenderer.setTheme(getTheme());
            }
            public override int getColumnSpan()
            {
                return 2;
            }
            public override Widget getCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                bgRenderer.setPosition(x, y);
                bgRenderer.setSize(width, height);
                int indent = getIndentation();
                textRenderer.setPosition(x + indent, y);
                textRenderer.setSize(Math.Max(0, width - indent), height);
                bgRenderer.getAnimationState().setAnimationState(STATE_SELECTED, isSelected);
                return bgRenderer;
            }
            public override void setCellData(int row, int column, Object data, NodeState nodeState)
            {
                base.setCellData(row, column, data, nodeState);
                textRenderer.setText((String)data);
            }
            protected override void setSubRenderer(int row, int column, Object colData)
            {
            }
        }

        class EditorRenderer : CellRenderer, TreeTable.CellWidgetCreator
        {
            private PropertyEditor editor;

            public void applyTheme(ThemeInfo themeInfo)
            {
            }
            public Widget getCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                editor.setSelected(isSelected);
                return null;
            }
            public int getColumnSpan()
            {
                return 1;
            }
            public int getPreferredHeight()
            {
                return editor.getWidget().getPreferredHeight();
            }
            public String getTheme()
            {
                return "PropertyEditorCellRender";
            }
            public void setCellData(int row, int column, Object data)
            {
                editor = (PropertyEditor)data;
            }
            public Widget updateWidget(Widget existingWidget)
            {
                return editor.getWidget();
            }
            public void positionWidget(Widget widget, int x, int y, int w, int h)
            {
                if (!editor.positionWidget(x, y, w, h))
                {
                    widget.setPosition(x, y);
                    widget.setSize(w, h);
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

            public void addChild(TreeTableNode parent)
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
            private EditField editField;
            private Property<String> property;

            public StringEditor(Property<String> property)
            {
                this.property = property;
                this.editField = new EditField();
                editField.Callback += EditField_Callback;
                resetValue();
            }

            private void EditField_Callback(object sender, EditFieldCallbackEventArgs e)
            {
                if (e.Key == Event.KEY_ESCAPE)
                {
                    resetValue();
                }
                else if (!property.IsReadOnly)
                {
                    try
                    {
                        property.Value = editField.getText();
                        editField.setErrorMessage(null);
                    }
                    catch (ArgumentException ex)
                    {
                        editField.setErrorMessage(ex.Message);
                    }
                }
            }

            public Widget getWidget()
            {
                return editField;
            }
            public void valueChanged()
            {
                resetValue();
            }
            public void preDestroy()
            {
                editField.Callback -= EditField_Callback;
            }
            public void setSelected(bool selected)
            {
            }
            private void resetValue()
            {
                editField.setText(property.Value);
                editField.setErrorMessage(null);
                editField.setReadOnly(property.IsReadOnly);
            }
            public bool positionWidget(int x, int y, int width, int height)
            {
                return false;
            }
        }

        class StringEditorFactory : PropertyEditorFactory
        {
            public PropertyEditor createEditor(Property<T> property)
            {
                if (property is Property<string>)
                {
                    return new StringEditor((Property<string>) property);
                }

                throw new Exception("StringEditorFactory given property that isn't of string generic");
            }
        }

        public class ComboBoxEditor<T> : PropertyEditor
        {
            protected ComboBox<T> comboBox;
            protected Property<T> property;
            protected ListModel<T> model;

            public ComboBoxEditor(Property<T> property, ListModel<T> model)
            {
                this.property = property;
                this.comboBox = new ComboBox<T>(model);
                this.model = model;
                comboBox.SelectionChanged += ComboBox_SelectionChanged;
            }

            private void ComboBox_SelectionChanged(object sender, ComboBoxSelectionChangedEventArgs e)
            {
                if (property.IsReadOnly)
                {
                    resetValue();
                }
                else
                {
                    int idx = comboBox.getSelected();
                    if (idx >= 0)
                    {
                        property.Value = model.EntryAt(idx);
                    }
                }
            }

            public Widget getWidget()
            {
                return comboBox;
            }

            public void valueChanged()
            {
                resetValue();
            }

            public void preDestroy()
            {
                comboBox.SelectionChanged -= ComboBox_SelectionChanged;
            }

            public void setSelected(bool selected)
            {
            }

            public void run()
            {
                if (property.IsReadOnly)
                {
                    resetValue();
                }
                else
                {
                    int idx = comboBox.getSelected();
                    if (idx >= 0)
                    {
                        property.Value = (model.EntryAt(idx));
                    }
                }
            }

            protected void resetValue()
            {
                comboBox.setSelected(findEntry(property.Value));
            }

            protected int findEntry(T value)
            {
                for (int i = 0, n = model.Entries; i < n; i++)
                {
                    if (model.EntryAt(i).Equals(value))
                    {
                        return i;
                    }
                }
                return -1;
            }

            public bool positionWidget(int x, int y, int width, int height)
            {
                return false;
            }
        }

        public class ComboBoxEditorFactory : PropertyEditorFactory
        {
            private ModelForwarder modelForwarder;
            public ComboBoxEditorFactory(ListModel<T> model)
            {
                this.modelForwarder = new ModelForwarder(model);
            }

            public ListModel<T> getModel()
            {
                return modelForwarder.getModel();
            }

            public void setModel(ListModel<T> model)
            {
                modelForwarder.setModel(model);
            }

            public PropertyEditor createEditor(Property<T> property)
            {
                return new ComboBoxEditor<T>(property, modelForwarder);
            }

            class ModelForwarder : AbstractListModel<T>
            {
                private ListModel<T> model;

                public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
                public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
                public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
                public override event EventHandler<ListAllChangedEventArgs> AllChanged;

                public override int Entries
                {
                    get
                    {
                        return model.Entries;
                    }
                }

                public ModelForwarder(ListModel<T> model)
                {
                    setModel(model);
                }

                public ListModel<T> getModel()
                {
                    return model;
                }

                public void setModel(ListModel<T> model)
                {
                    if (this.model != null)
                    {
                        this.model.EntriesChanged -= Model_EntriesChanged;
                        this.model.EntriesDeleted -= Model_EntriesDeleted;
                        this.model.EntriesInserted -= Model_EntriesInserted;
                        this.model.AllChanged -= Model_AllChanged;
                    }
                    this.model = model;
                    this.model.EntriesChanged += Model_EntriesChanged;
                    this.model.EntriesDeleted += Model_EntriesDeleted;
                    this.model.EntriesInserted += Model_EntriesInserted;
                    this.model.AllChanged += Model_AllChanged;
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
                    return model.EntryAt(index);
                }

                public override bool EntryMatchesPrefix(int index, string prefix)
                {
                    return model.EntryMatchesPrefix(index, prefix);
                }

                public override object EntryTooltipAt(int index)
                {
                    return model.EntryTooltipAt(index);
                }
            }
        }
    }
}
