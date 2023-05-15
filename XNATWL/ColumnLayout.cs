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

namespace XNATWL
{
    public class ColumnLayout : DialogLayout
    {
        List<Group> _columnGroups;
        private Panel _rootPanel;
        private Dictionary<Columns, Columns> _columns;

        public ColumnLayout()
        {
            this._columnGroups = new List<Group>();
            this._rootPanel = new Panel(this, null);
            this._columns = new Dictionary<Columns, Columns>();

            SetHorizontalGroup(CreateParallelGroup());
            SetVerticalGroup(_rootPanel._rows);
        }

        public Panel GetRootPanel()
        {
            return _rootPanel;
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
        public Columns GetColumns(params String[] columnNames)
        {
            if (columnNames.Length == 0)
            {
                throw new ArgumentOutOfRangeException("columnNames");
            }
            Columns key = new Columns(columnNames);
            Columns cl = _columns[key];
            if (cl != null)
            {
                return cl;
            }
            CreateColumns(key);
            return key;
        }

        /**
         * Adds a new row. This is a short cut for {@code getRootPanel().addRow(columns)}
         *
         * @param columns the column layout info
         * @return the new row
         */
        public Row AddRow(Columns columns)
        {
            return _rootPanel.AddRow(columns);
        }

        /**
         * Adds a new row. This is a short cut for
         * {@code getRootPanel().addRow(getColumns(columnNames))}
         *
         * @param columnNames the column names
         * @return the new row
         */
        public Row AddRow(params String[] columnNames)
        {
            return _rootPanel.AddRow(GetColumns(columnNames));
        }

        private void CreateColumns(Columns cl)
        {
            int prefixSize = 0;
            Columns prefixColumns = null;
            foreach (Columns c in _columns.Values)
            {
                int match = c.Match(cl);
                if (match > prefixSize)
                {
                    prefixSize = match;
                    prefixColumns = c;
                }
            }

            int numColumns = 0;
            for (int i = 0, n = cl._names.Length; i < n; i++)
            {
                if (!cl.IsGap(i))
                {
                    numColumns++;
                }
            }

            cl._numColumns = numColumns;
            cl._firstColumn = _columnGroups.Count;
            cl._childGroups = new Group[cl._names.Length];
            Group h = CreateSequentialGroup();

            if (prefixColumns == null)
            {
                GetHorizontalGroup().AddGroup(h);
            }
            else
            {
                for (int i = 0; i < prefixSize; i++)
                {
                    if (!cl.IsGap(i))
                    {
                        Group g = _columnGroups[prefixColumns._firstColumn + i];
                        _columnGroups.Add(g);
                    }
                }
                Array.Copy(prefixColumns._childGroups, 0, cl._childGroups, 0, prefixSize);
                cl._childGroups[prefixSize - 1].AddGroup(h);
            }

            for (int i = prefixSize, n = cl._names.Length; i < n; i++)
            {
                if (cl.IsGap(i))
                {
                    h.AddGap();
                }
                else
                {
                    Group g = CreateParallelGroup();
                    h.AddGroup(g);
                    _columnGroups.Add(g);
                }
                Group nextSequential = CreateSequentialGroup();
                Group childGroup = CreateParallelGroup().AddGroup(nextSequential);
                h.AddGroup(childGroup);
                h = nextSequential;
                cl._childGroups[i] = childGroup;
            }
            _columns.Add(cl, cl);
        }

        public class Columns
        {
            protected internal String[] _names;
            protected internal int _hashCode;
            protected internal int _firstColumn;
            protected internal int _numColumns;
            protected internal Group[] _childGroups;

            protected internal Columns(String[] names)
            {
                this._names = (String[]) names.Clone();

                unchecked
                {
                    int hash = 17;

                    // get hash code for all items in array
                    foreach (var item in this._names)
                    {
                        hash = hash * 23 + ((item != null) ? item.GetHashCode() : 0);
                    }

                    this._hashCode = hash;
                }
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                Columns other = (Columns)obj;
                return this._hashCode == other._hashCode && this._names == other._names;
            }

            /**
             * Returns the number of non gap columns.
             * @return the number of non gap columns.
             */
            public int GetNumColumns()
            {
                return _numColumns;
            }

            public int GetNumColumnNames()
            {
                return _names.Length;
            }

            public String GetColumnName(int idx)
            {
                return _names[idx];
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            protected internal bool IsGap(int column)
            {
                String name = _names[column];
                return name.Length == 0 || "-".Equals(name);
            }

            protected internal int Match(Columns other)
            {
                int cnt = Math.Min(this._names.Length, other._names.Length);
                for (int i = 0; i < cnt; i++)
                {
                    if (!_names[i].Equals(other._names[i]))
                    {
                        return i;
                    }
                }
                return cnt;
            }
        }

        public class Row
        {
            private ColumnLayout _columnLayout;
            Columns _columns;
            Panel _panel;
            Group _row;
            int _curColumn;

            protected internal Row(ColumnLayout columnLayout, Columns columns, Panel panel, Group row)
            {
                this._columnLayout = columnLayout;
                this._columns = columns;
                this._panel = panel;
                this._row = row;
            }

            /**
             * Returns the current column. Adding a widget increments the this.
             * @return the current column.
             * @see Columns#getNumColumns()
             */
            public int GetCurrentColumn()
            {
                return _curColumn;
            }

            public Columns GetColumns()
            {
                return _columns;
            }

            /**
             * Adds a new widget to the row using {@link Alignment#FILL} alignment.
             *
             * @param w the new widget
             * @return this
             * @throws IllegalStateException if all widgets for this row have already been added
             */
            public Row Add(Widget w)
            {
                if (_curColumn == _columns._numColumns)
                {
                    throw new ArgumentOutOfRangeException("Too many widgets for column layout");
                }
                _panel.GetColumn(_columns._firstColumn + _curColumn).AddWidget(w);
                _row.AddWidget(w);
                _curColumn++;
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
            public Row Add(Widget w, Alignment alignment)
            {
                Add(w);
                this._columnLayout.SetWidgetAlignment(w, alignment);
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
            public Row AddLabel(String labelText)
            {
                if (labelText == null)
                {
                    throw new ArgumentNullException("labelText");
                }
                return Add(new Label(labelText));
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
            public Row AddWithLabel(String labelText, Widget w)
            {
                if (labelText == null)
                {
                    throw new ArgumentNullException("labelText");
                }
                Label labelWidget = new Label(labelText);
                labelWidget.SetLabelFor(w);
                Add(labelWidget, Alignment.TOPLEFT).Add(w);
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
            public Row AddWithLabel(String labelText, Widget w, Alignment alignment)
            {
                AddWithLabel(labelText, w);
                this._columnLayout.SetWidgetAlignment(w, alignment);
                return this;
            }
        }

        public class Panel
        {
            protected internal Panel _parent;
            protected internal List<Group> _usedColumnGroups;
            protected internal List<Panel> _children;
            protected internal Group _rows;
            protected internal bool _valid;
            private ColumnLayout _columnLayout;

            protected internal Panel(ColumnLayout columnLayout, Panel parent)
            {
                this._columnLayout = columnLayout;
                this._parent = parent;
                this._usedColumnGroups = new List<Group>();
                this._children = new List<Panel>();
                this._rows = this._columnLayout.CreateSequentialGroup();
                this._valid = true;
            }

            public bool IsValid()
            {
                return _valid;
            }

            /**
             * Calls {@link ColumnLayout#getColumns(java.lang.String[]) }
             *
             * @param columnNames the column names.
             * @return the column layout.
             */
            public Columns GetColumns(params String[] columnNames)
            {
                return this._columnLayout.GetColumns(columnNames);
            }

            /**
             * Adds a new row to this panel using the specified column names.
             *
             * <p>It is equivalent to {@code addRow(getColumns(columnNames))}</p>
             *
             * @param columnNames the column names.
             * @return the new row
             */
            public Row AddRow(params String[] columnNames)
            {
                return AddRow(this._columnLayout.GetColumns(columnNames));
            }

            /**
             * Adds a new row to this panel.
             *
             * @param columns the column layout
             * @return the new row.
             * @throws IllegalStateException when the panel has been removed from the root.
             */
            public Row AddRow(Columns columns)
            {
                if (columns == null)
                {
                    throw new ArgumentOutOfRangeException("columns");
                }
                CheckValid();
                Group row = this._columnLayout.CreateParallelGroup();
                _rows.AddGroup(row);
                return new Row(this._columnLayout, columns, this, row);
            }

            /**
             * Adds a named vertical gap.
             *
             * @param name the gap name.
             * @throws IllegalStateException when the panel has been removed from the root.
             */
            public void AddVerticalGap(String name)
            {
                CheckValid();
                _rows.AddGap(name);
            }

            /**
             * Adds a new child panel
             *
             * @return the new child panel
             * @throws IllegalStateException when the panel has been removed from the root.
             */
            public Panel AddPanel()
            {
                CheckValid();
                Panel panel = new Panel(this._columnLayout, this);
                _rows.AddGroup(panel._rows);
                _children.Add(panel);
                return panel;
            }

            /**
             * Removes the specified child panel. Can also be called on an invalidated panel.
             * @param panel the child panel.
             */
            public void RemovePanel(Panel panel)
            {
                if (panel == null)
                {
                    throw new ArgumentNullException("panel");
                }
                if (_valid)
                {
                    if (_children.Contains(panel))
                    {
                        _children.Remove(panel);
                        panel.MarkInvalid();
                        _rows.RemoveGroup(panel._rows, true);
                        for (int i = 0, n = panel._usedColumnGroups.Count; i < n; i++)
                        {
                            Group column = panel._usedColumnGroups[i];
                            if (column != null)
                            {
                                _usedColumnGroups[i].RemoveGroup(column, false);
                            }
                        }
                    }
                }
            }

            /**
             * Removes all child panels and rows from this panel.
             */
            public void ClearPanel()
            {
                if (_valid)
                {
                    _children.Clear();
                    _rows.Clear(true);
                    for (int i = 0, n = _usedColumnGroups.Count; i < n; i++)
                    {
                        Group column = _usedColumnGroups[i];
                        if (column != null)
                        {
                            column.Clear(false);
                        }
                    }
                }
            }

            protected internal void MarkInvalid()
            {
                _valid = false;
                for (int i = 0, n = _children.Count; i < n; i++)
                {
                    _children[i].MarkInvalid();
                }
            }

            protected internal void CheckValid()
            {
                if (!_valid)
                {
                    throw new InvalidOperationException("Panel has been removed");
                }
            }

            protected internal Group GetColumn(int idx)
            {
                CheckValid();
                if (_usedColumnGroups.Count > idx)
                {
                    Group column = _usedColumnGroups[idx];
                    if (column != null)
                    {
                        return column;
                    }
                }
                return MakeColumn(idx);
            }

            private Group MakeColumn(int idx)
            {
                Group parentColumn;
                if (_parent != null)
                {
                    parentColumn = _parent.GetColumn(idx);
                }
                else
                {
                    parentColumn = this._columnLayout._columnGroups[idx];
                }
                Group column = this._columnLayout.CreateParallelGroup();
                parentColumn.AddGroup(column);
                while (_usedColumnGroups.Count <= idx)
                {
                    _usedColumnGroups.Add(null);
                }
                _usedColumnGroups[idx] = column;
                return column;
            }
        }
    }

}
