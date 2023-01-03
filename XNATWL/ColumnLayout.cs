using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{

    public class ColumnLayout : DialogLayout
    {
        List<Group> columnGroups;
        private Panel rootPanel;
        private Dictionary<Columns, Columns> columns;

        public ColumnLayout()
        {
            this.columnGroups = new List<Group>();
            this.rootPanel = new Panel(this, null);
            this.columns = new Dictionary<Columns, Columns>();

            setHorizontalGroup(createParallelGroup());
            setVerticalGroup(rootPanel.rows);
        }

        public Panel getRootPanel()
        {
            return rootPanel;
        }

        /**
         * Returns the column layout for the specified list of columns.
         *
         * <p>A column name of {@code ""} or {@code "-"} is used to create a
         * flexible gap.</p>
         *
         * <p>Layouts are merged starting with the first column if that column
         * name is already in use. Merged column layouts share the column width.</p>
         * 
         * @param columnNames list of column names
         * @return the column layout
         */
        public Columns getColumns(params String[] columnNames)
        {
            if (columnNames.Length == 0)
            {
                throw new ArgumentOutOfRangeException("columnNames");
            }
            Columns key = new Columns(columnNames);
            Columns cl = columns[key];
            if (cl != null)
            {
                return cl;
            }
            createColumns(key);
            return key;
        }

        /**
         * Adds a new row. This is a short cut for {@code getRootPanel().addRow(columns)}
         *
         * @param columns the column layout info
         * @return the new row
         */
        public Row addRow(Columns columns)
        {
            return rootPanel.addRow(columns);
        }

        /**
         * Adds a new row. This is a short cut for
         * {@code getRootPanel().addRow(getColumns(columnNames))}
         *
         * @param columnNames the column names
         * @return the new row
         */
        public Row addRow(params String[] columnNames)
        {
            return rootPanel.addRow(getColumns(columnNames));
        }

        private void createColumns(Columns cl)
        {
            int prefixSize = 0;
            Columns prefixColumns = null;
            foreach (Columns c in columns.Values)
            {
                int match = c.match(cl);
                if (match > prefixSize)
                {
                    prefixSize = match;
                    prefixColumns = c;
                }
            }

            int numColumns = 0;
            for (int i = 0, n = cl.names.Length; i < n; i++)
            {
                if (!cl.isGap(i))
                {
                    numColumns++;
                }
            }

            cl.numColumns = numColumns;
            cl.firstColumn = columnGroups.Count;
            cl.childGroups = new Group[cl.names.Length];
            Group h = createSequentialGroup();

            if (prefixColumns == null)
            {
                getHorizontalGroup().addGroup(h);
            }
            else
            {
                for (int i = 0; i < prefixSize; i++)
                {
                    if (!cl.isGap(i))
                    {
                        Group g = columnGroups[prefixColumns.firstColumn + i];
                        columnGroups.Add(g);
                    }
                }
                Array.Copy(prefixColumns.childGroups, 0, cl.childGroups, 0, prefixSize);
                cl.childGroups[prefixSize - 1].addGroup(h);
            }

            for (int i = prefixSize, n = cl.names.Length; i < n; i++)
            {
                if (cl.isGap(i))
                {
                    h.addGap();
                }
                else
                {
                    Group g = createParallelGroup();
                    h.addGroup(g);
                    columnGroups.Add(g);
                }
                Group nextSequential = createSequentialGroup();
                Group childGroup = createParallelGroup().addGroup(nextSequential);
                h.addGroup(childGroup);
                h = nextSequential;
                cl.childGroups[i] = childGroup;
            }
            columns.Add(cl, cl);
        }

        public class Columns
        {
            protected internal String[] names;
            protected internal int hashcode;
            protected internal int firstColumn;
            protected internal int numColumns;
            protected internal Group[] childGroups;

            protected internal Columns(String[] names)
            {
                this.names = (String[]) names.Clone();

                unchecked
                {
                    int hash = 17;

                    // get hash code for all items in array
                    foreach (var item in this.names)
                    {
                        hash = hash * 23 + ((item != null) ? item.GetHashCode() : 0);
                    }

                    this.hashcode = hash;
                }
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                Columns other = (Columns)obj;
                return this.hashcode == other.hashcode &&
                        this.names == other.names;
            }

            /**
             * Returns the number of non gap columns.
             * @return the number of non gap columns.
             */
            public int getNumColumns()
            {
                return numColumns;
            }

            public int getNumColumnNames()
            {
                return names.Length;
            }

            public String getColumnName(int idx)
            {
                return names[idx];
            }

            public override int GetHashCode()
            {
                return hashcode;
            }

            protected internal bool isGap(int column)
            {
                String name = names[column];
                return name.Length == 0 || "-".Equals(name);
            }

            protected internal int match(Columns other)
            {
                int cnt = Math.Min(this.names.Length, other.names.Length);
                for (int i = 0; i < cnt; i++)
                {
                    if (!names[i].Equals(other.names[i]))
                    {
                        return i;
                    }
                }
                return cnt;
            }
        }

        public class Row
        {
            private ColumnLayout columnLayout;
            Columns columns;
            Panel panel;
            Group row;
            int curColumn;

            protected internal Row(ColumnLayout columnLayout, Columns columns, Panel panel, Group row)
            {
                this.columnLayout = columnLayout;
                this.columns = columns;
                this.panel = panel;
                this.row = row;
            }

            /**
             * Returns the current column. Adding a widget increments the this.
             * @return the current column.
             * @see Columns#getNumColumns()
             */
            public int getCurrentColumn()
            {
                return curColumn;
            }

            public Columns getColumns()
            {
                return columns;
            }

            /**
             * Adds a new widget to the row using {@link Alignment#FILL} alignment.
             *
             * @param w the new widget
             * @return this
             * @throws IllegalStateException if all widgets for this row have already been added
             */
            public Row add(Widget w)
            {
                if (curColumn == columns.numColumns)
                {
                    throw new ArgumentOutOfRangeException("Too many widgets for column layout");
                }
                panel.getColumn(columns.firstColumn + curColumn).addWidget(w);
                row.addWidget(w);
                curColumn++;
                return this;
            }

            /**
             * Adds a new widget to this row using the specified alignment.
             *
             * @param w the new widget
             * @param alignment the alignment for the new widget
             * @return this
             * @throws IllegalStateException if all widgets for this row have already been added
             */
            public Row add(Widget w, Alignment alignment)
            {
                add(w);
                this.columnLayout.setWidgetAlignment(w, alignment);
                return this;
            }

            /**
             * Adds a new label to this row. The label is not associate with any widget.
             *
             * <p>It is equivalent to {@code add(new Label(label))}.</p>
             *
             * @param labelText the label text
             * @return this
             * @throws IllegalStateException if all widgets for this row have already been added
             */
            public Row addLabel(String labelText)
            {
                if (labelText == null)
                {
                    throw new ArgumentNullException("labelText");
                }
                return add(new Label(labelText));
            }

            /**
             * Adds a label followed by the specified widget. The label uses
             * {@link Alignment#TOPLEFT} alignment, and is associated to the widget.
             * The alignment of the widget is {@link Alignment#FILL}.
             *
             * @param labelText the label text
             * @param w the new widget
             * @return this
             * @throws IllegalStateException if all widgets for this row have already been added
             */
            public Row addWithLabel(String labelText, Widget w)
            {
                if (labelText == null)
                {
                    throw new ArgumentNullException("labelText");
                }
                Label labelWidget = new Label(labelText);
                labelWidget.setLabelFor(w);
                add(labelWidget, Alignment.TOPLEFT).add(w);
                return this;
            }

            /**
             * Adds a label followed by the specified widget. The label uses
             * {@link Alignment#TOPLEFT} alignment, and is associated to the widget.
             *
             * @param labelText the label text
             * @param w the new widget
             * @param alignment the alignment for the new widget
             * @return this
             * @throws IllegalStateException if all widgets for this row have already been added
             */
            public Row addWithLabel(String labelText, Widget w, Alignment alignment)
            {
                addWithLabel(labelText, w);
                this.columnLayout.setWidgetAlignment(w, alignment);
                return this;
            }
        }

        public class Panel
        {
            protected internal Panel parent;
            protected internal List<Group> usedColumnGroups;
            protected internal List<Panel> children;
            protected internal Group rows;
            protected internal bool valid;
            private ColumnLayout columnLayout;

            protected internal Panel(ColumnLayout columnLayout, Panel parent)
            {
                this.columnLayout = columnLayout;
                this.parent = parent;
                this.usedColumnGroups = new List<Group>();
                this.children = new List<Panel>();
                this.rows = this.columnLayout.createSequentialGroup();
                this.valid = true;
            }

            public bool isValid()
            {
                return valid;
            }

            /**
             * Calls {@link ColumnLayout#getColumns(java.lang.String[]) }
             *
             * @param columnNames the column names.
             * @return the column layout.
             */
            public Columns getColumns(params String[] columnNames)
            {
                return this.columnLayout.getColumns(columnNames);
            }

            /**
             * Adds a new row to this panel using the specified column names.
             *
             * <p>It is equivalent to {@code addRow(getColumns(columnNames))}</p>
             *
             * @param columnNames the column names.
             * @return the new row
             */
            public Row addRow(params String[] columnNames)
            {
                return addRow(this.columnLayout.getColumns(columnNames));
            }

            /**
             * Adds a new row to this panel.
             *
             * @param columns the column layout
             * @return the new row.
             * @throws IllegalStateException when the panel has been removed from the root.
             */
            public Row addRow(Columns columns)
            {
                if (columns == null)
                {
                    throw new ArgumentOutOfRangeException("columns");
                }
                checkValid();
                Group row = this.columnLayout.createParallelGroup();
                rows.addGroup(row);
                return new Row(this.columnLayout, columns, this, row);
            }

            /**
             * Adds a named vertical gap.
             *
             * @param name the gap name.
             * @throws IllegalStateException when the panel has been removed from the root.
             */
            public void addVerticalGap(String name)
            {
                checkValid();
                rows.addGap(name);
            }

            /**
             * Adds a new child panel
             *
             * @return the new child panel
             * @throws IllegalStateException when the panel has been removed from the root.
             */
            public Panel addPanel()
            {
                checkValid();
                Panel panel = new Panel(this.columnLayout, this);
                rows.addGroup(panel.rows);
                children.Add(panel);
                return panel;
            }

            /**
             * Removes the specified child panel. Can also be called on an invalidated panel.
             * @param panel the child panel.
             */
            public void removePanel(Panel panel)
            {
                if (panel == null)
                {
                    throw new ArgumentNullException("panel");
                }
                if (valid)
                {
                    if (children.Contains(panel))
                    {
                        children.Remove(panel);
                        panel.markInvalid();
                        rows.removeGroup(panel.rows, true);
                        for (int i = 0, n = panel.usedColumnGroups.Count; i < n; i++)
                        {
                            Group column = panel.usedColumnGroups[i];
                            if (column != null)
                            {
                                usedColumnGroups[i].removeGroup(column, false);
                            }
                        }
                    }
                }
            }

            /**
             * Removes all child panels and rows from this panel.
             */
            public void clearPanel()
            {
                if (valid)
                {
                    children.Clear();
                    rows.clear(true);
                    for (int i = 0, n = usedColumnGroups.Count; i < n; i++)
                    {
                        Group column = usedColumnGroups[i];
                        if (column != null)
                        {
                            column.clear(false);
                        }
                    }
                }
            }

            protected internal void markInvalid()
            {
                valid = false;
                for (int i = 0, n = children.Count; i < n; i++)
                {
                    children[i].markInvalid();
                }
            }

            protected internal void checkValid()
            {
                if (!valid)
                {
                    throw new InvalidOperationException("Panel has been removed");
                }
            }

            protected internal Group getColumn(int idx)
            {
                checkValid();
                if (usedColumnGroups.Count > idx)
                {
                    Group column = usedColumnGroups[idx];
                    if (column != null)
                    {
                        return column;
                    }
                }
                return makeColumn(idx);
            }

            private Group makeColumn(int idx)
            {
                Group parentColumn;
                if (parent != null)
                {
                    parentColumn = parent.getColumn(idx);
                }
                else
                {
                    parentColumn = this.columnLayout.columnGroups[idx];
                }
                Group column = this.columnLayout.createParallelGroup();
                parentColumn.addGroup(column);
                while (usedColumnGroups.Count <= idx)
                {
                    usedColumnGroups.Add(null);
                }
                usedColumnGroups[idx] = column;
                return column;
            }
        }
    }

}
