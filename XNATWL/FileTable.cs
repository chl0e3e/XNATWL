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
using System.Globalization;
using System.Text;
using XNATWL.Model;

namespace XNATWL
{
    public class FileTable : Table
    {
        public enum SortColumn
        {
            Name,
            Type,
            Size,
            LastModified
        }

        public static IComparer<Entry> SortColumn_Comparer(SortColumn sortColumn)
        {
            switch(sortColumn)
            {
                case SortColumn.Name:
                    return NameComparator.instance;
                case SortColumn.Type:
                    return ExtensionComparator.instance;
                case SortColumn.Size:
                    return SizeComparator.instance;
                case SortColumn.LastModified:
                    return LastModifiedComparator.instance;
                default:
                    return NameComparator.instance;
            }
        }

        private FileTableModel _fileTableModel;
        private TableSelectionModel _fileTableSelectionModel;
        private TableSearchWindow _tableSearchWindow;
        private SortColumn _sortColumn = SortColumn.Name;
        private SortOrder _sortOrder = SortOrder.Ascending;

        private bool _allowMultiSelection;
        private FileFilter _fileFilter = null;
        private bool _showFolders = true;
        private bool _showHidden = false;

        private FileSystemModel _fileSystemModel;
        private Object _currentFolder;

        public event EventHandler<FileTableSelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<FileTableSortingChangedEventArgs> SortingChanged;

        public FileTable()
        {
            _fileTableModel = new FileTableModel(this);
            SetModel(_fileTableModel);

            /*selectionChangedListener = new Runnable() {
                public void run() {
                    selectionChanged();
                }
            };*/
        }

        public bool GetShowFolders()
        {
            return _showFolders;
        }

        public void SetShowFolders(bool showFolders)
        {
            if (this._showFolders != showFolders)
            {
                this._showFolders = showFolders;
                RefreshFileTable();
            }
        }

        public bool GetShowHidden()
        {
            return _showHidden;
        }

        public void SetShowHidden(bool showHidden)
        {
            if (this._showHidden != showHidden)
            {
                this._showHidden = showHidden;
                RefreshFileTable();
            }
        }

        public void SetFileFilter(FileFilter filter)
        {
            // always refresh, filter parameters could have been changed
            _fileFilter = filter;
            RefreshFileTable();
        }

        public FileFilter GetFileFilter()
        {
            return _fileFilter;
        }

        public Entry[] GetSelection()
        {
            return _fileTableModel.GetEntries(_fileTableSelectionModel.Selection);
        }

        public void SetSelection(params Object[] files)
        {
            _fileTableSelectionModel.ClearSelection();
            foreach (Object file in files)
            {
                int idx = _fileTableModel.FindFile(file);
                if (idx >= 0)
                {
                    _fileTableSelectionModel.AddSelection(idx, idx);
                }
            }
        }

        public bool SetSelection(Object file)
        {
            _fileTableSelectionModel.ClearSelection();
            int idx = _fileTableModel.FindFile(file);
            if (idx >= 0)
            {
                _fileTableSelectionModel.AddSelection(idx, idx);
                ScrollToRow(idx);
                return true;
            }
            return false;
        }

        public void ClearSelection()
        {
            _fileTableSelectionModel.ClearSelection();
        }

        public void SetSortColumn(SortColumn column)
        {
            if (_sortColumn != column)
            {
                _sortColumn = column;
                FireSortingChanged();
            }
        }

        public void SetSortOrder(SortOrder order)
        {
            if (_sortOrder != order)
            {
                _sortOrder = order;
                FireSortingChanged();
            }
        }

        public bool GetAllowMultiSelection()
        {
            return _allowMultiSelection;
        }

        public void SetAllowMultiSelection(bool allowMultiSelection)
        {
            this._allowMultiSelection = allowMultiSelection;
            if (_fileTableSelectionModel != null)
            {
                _fileTableSelectionModel.SelectionChanged -= FileTableSelectionModel_SelectionChanged;
            }
            if (_tableSearchWindow != null)
            {
                _tableSearchWindow.SetModel(null, 0);
            }
            if (allowMultiSelection)
            {
                _fileTableSelectionModel = new DefaultTableSelectionModel();
            }
            else
            {
                _fileTableSelectionModel = new TableSingleSelectionModel();
            }
            _fileTableSelectionModel.SelectionChanged += FileTableSelectionModel_SelectionChanged;
            _tableSearchWindow = new TableSearchWindow(this, _fileTableSelectionModel);
            _tableSearchWindow.SetModel(_fileTableModel, 0);
            SetSelectionManager(new TableRowSelectionManager(_fileTableSelectionModel));
            SetKeyboardSearchHandler(_tableSearchWindow);
            FireSelectionChanged();
        }

        private void FileTableSelectionModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FireSelectionChanged();
        }

        public FileSystemModel GetFileSystemModel()
        {
            return _fileSystemModel;
        }

        public Object GetCurrentFolder()
        {
            return _currentFolder;
        }

        public bool IsRoot()
        {
            return _currentFolder == null;
        }

        public void SetCurrentFolder(FileSystemModel fsm, Object folder)
        {
            this._fileSystemModel = fsm;
            this._currentFolder = folder;
            RefreshFileTable();
        }

        public void RefreshFileTable()
        {
            Object[] objs = CollectObjects();
            if (objs != null)
            {
                int lastFileIdx = objs.Length;
                Entry[] entries = new Entry[lastFileIdx];
                int numFolders = 0;
                bool bIsRoot = IsRoot();
                for (int i = 0; i < objs.Length; i++)
                {
                    Entry e = new Entry(_fileSystemModel, objs[i], bIsRoot);
                    if (e.IsFolder)
                    {
                        entries[numFolders++] = e;
                    }
                    else
                    {
                        entries[--lastFileIdx] = e;
                    }
                }
                Array.Sort(entries, 0, numFolders, NameComparator.instance);
                SortFilesAndUpdateModel(entries, numFolders);
            }
            else
            {
                SortFilesAndUpdateModel(EMPTY, 0);
            }

            if (_tableSearchWindow != null)
            {
                _tableSearchWindow.CancelSearch();
            }
        }

        protected void FireSelectionChanged()
        {
            this.SelectionChanged.Invoke(this, new FileTableSelectionChangedEventArgs());
        }

        protected void FireSortingChanged()
        {
            SetSortArrows();
            SortFilesAndUpdateModel();
            this.SortingChanged.Invoke(this, new FileTableSortingChangedEventArgs());
        }

        private Object[] CollectObjects()
        {
            if (_fileSystemModel == null)
            {
                return null;
            }
            if (IsRoot())
            {
                return _fileSystemModel.ListRoots();
            }
            FileFilter filter = _fileFilter;
            if (filter != null || !GetShowFolders() || !GetShowHidden())
            {
                filter = new FileFilterWrapper(filter, GetShowFolders(), GetShowHidden());
            }
            return _fileSystemModel.ListFolder(_currentFolder, filter);
        }

        private void SortFilesAndUpdateModel(Entry[] entries, int numFolders)
        {
            StateSnapshot snapshot = MakeSnapshot();
            Array.Sort(entries, numFolders, entries.Length, SortColumn_Comparer(_sortColumn));
            _fileTableModel.SetData(entries, numFolders);
            RestoreSnapshot(snapshot);
        }

        protected override void ColumnHeaderClicked(int column)
        {
            base.ColumnHeaderClicked(column);

            SortColumn thisColumn = (SortColumn) Enum.GetValues(typeof(SortColumn)).GetValue(column);
            if (_sortColumn == thisColumn)
            {
                SetSortOrder(SortOrderStatics.SortOrder_Invert(_sortOrder));
            }
            else
            {
                SetSortColumn(thisColumn);
            }
        }

        protected override void UpdateColumnHeaderNumbers()
        {
            base.UpdateColumnHeaderNumbers();
            SetSortArrows();
        }

        protected void SetSortArrows()
        {
            int i = 0;

            foreach (SortColumn column in Enum.GetValues(typeof(SortColumn)))
            {
                if (column == _sortColumn)
                {
                    break;
                }
                i++;
            }

            SetColumnSortOrderAnimationState(i, _sortOrder);
        }

        private void SortFilesAndUpdateModel()
        {
            SortFilesAndUpdateModel(_fileTableModel._entries, _fileTableModel._numFolders);
        }

        private StateSnapshot MakeSnapshot()
        {
            return new StateSnapshot(
                    _fileTableModel.GetEntry(_fileTableSelectionModel.LeadIndex),
                    _fileTableModel.GetEntry(_fileTableSelectionModel.AnchorIndex),
                    _fileTableModel.GetEntries(_fileTableSelectionModel.Selection));
        }

        private void RestoreSnapshot(StateSnapshot snapshot)
        {
            foreach (Entry e in snapshot._selected)
            {
                int idx = _fileTableModel.FindEntry(e);
                if (idx >= 0)
                {
                    _fileTableSelectionModel.AddSelection(idx, idx);
                }
            }
            int leadIndex = _fileTableModel.FindEntry(snapshot._leadEntry);
            int anchorIndex = _fileTableModel.FindEntry(snapshot._anchorEntry);
            _fileTableSelectionModel.LeadIndex = leadIndex;
            _fileTableSelectionModel.AnchorIndex = anchorIndex;
            ScrollToRow(Math.Max(0, leadIndex));
        }

        static Entry[] EMPTY = new Entry[0];

        public class Entry
        {
            public FileSystemModel FSM;
            public Object Obj;
            public String Name;
            public bool IsFolder;
            public long Size;
            /** last modified date - can be null */
            public DateTime lastModified;

            public Entry(FileSystemModel fsm, Object obj, bool isRoot)
            {
                this.FSM = fsm;
                this.Obj = obj;
                this.Name = fsm.NameOf(obj);
                if (isRoot)
                {
                    // don't call getLastModified on roots - causes bad performance
                    // on windows when a DVD/CD/Floppy has no media inside
                    this.IsFolder = true;
                    this.lastModified = DateTime.MinValue;
                }
                else
                {
                    this.IsFolder = fsm.IsFolder(obj);
                    this.lastModified = new DateTime(fsm.LastModifiedOf(obj));
                }
                if (IsFolder)
                {
                    this.Size = 0;
                }
                else
                {
                    this.Size = fsm.SizeOf(obj);
                }
            }

            public String GetExtension()
            {
                int idx = Name.LastIndexOf('.');
                if (idx >= 0)
                {
                    return Name.Substring(idx + 1);
                }
                else
                {
                    return "";
                }
            }

            public String GetPath()
            {
                return FSM.PathOf(Obj);
            }

            public override bool Equals(Object o)
            {
                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }
                Entry that = (Entry)o;
                return (this.FSM == that.FSM) && FSM.Equals(this.Obj, that.Obj);
            }

            public override int GetHashCode()
            {
                return (Obj != null) ? Obj.GetHashCode() : 203;
            }
        }

        class FileTableModel : AbstractTableModel
        {
            //private DateFormat dateFormat = DateFormat.getDateInstance();
            private string _dateFormat = DateTimeFormatInfo.CurrentInfo.FullDateTimePattern;

            protected internal Entry[] _entries = EMPTY;
            protected internal int _numFolders;

            private FileTable _fileTable;

            public FileTableModel(FileTable fileTable)
            {
                this._fileTable = fileTable;
            }

            public void SetData(Entry[] entries, int numFolders)
            {
                this.FireRowsDeleted(0, this.Rows);
                this._entries = entries;
                this._numFolders = numFolders;
                this.FireRowsInserted(0, this.Rows);
            }

            static String[] COLUMN_HEADER = { "File name", "Type", "Size", "Last modified" };

            public override String ColumnHeaderTextFor(int column)
            {
                return COLUMN_HEADER[column];
            }

            public override Object CellAt(int row, int column)
            {
                Entry e = _entries[row];
                if (e.IsFolder)
                {
                    switch (column)
                    {
                        case 0: return "[" + e.Name + "]";
                        case 1: return "Folder";
                        case 2: return "";
                        case 3: return FormatDate(e.lastModified);
                        default: return "??";
                    }
                }
                else
                {
                    switch (column)
                    {
                        case 0: return e.Name;
                        case 1:
                            {
                                String ext = e.GetExtension();
                                return (ext.Length == 0) ? "File" : ext + "-file";
                            }
                        case 2: return FormatFileSize(e.Size);
                        case 3: return FormatDate(e.lastModified);
                        default: return "??";
                    }
                }
            }

            public override Object TooltipAt(int row, int column)
            {
                Entry e = _entries[row];
                StringBuilder sb = new StringBuilder(e.Name);
                if (!e.IsFolder)
                {
                    sb.Append("\nSize: ").Append(FormatFileSize(e.Size));
                }
                if (e.lastModified != null)
                {
                    sb.Append("\nLast modified: ").Append(FormatDate(e.lastModified));
                }
                return sb.ToString();
            }

            protected internal Entry GetEntry(int row)
            {
                if (row >= 0 && row < _entries.Length)
                {
                    return _entries[row];
                }
                else
                {
                    return null;
                }
            }

            protected internal int FindEntry(Entry entry)
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    if (_entries[i].Equals(entry))
                    {
                        return i;
                    }
                }
                return -1;
            }

            protected internal int FindFile(Object file)
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    Entry e = _entries[i];
                    if (e.FSM.Equals(e.Obj, file))
                    {
                        return i;
                    }
                }
                return -1;
            }

            protected internal Entry[] GetEntries(int[] selection)
            {
                int count = selection.Length;
                if (count == 0)
                {
                    return EMPTY;
                }
                Entry[] result = new Entry[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = _entries[selection[i]];
                }
                return result;
            }

            static String[] SIZE_UNITS = { " MB", " KB", " B" };
            static long[] SIZE_FACTORS = { 1024 * 1024, 1024, 1 };

            public override int Columns
            {
                get
                {
                    return COLUMN_HEADER.Length;
                }
            }

            public override int Rows
            {
                get
                {
                    return _entries.Length;
                }
            }

            private String FormatFileSize(long size)
            {
                if (size <= 0)
                {
                    return "0 B";
                }
                else
                {
                    for (int i = 0; ; ++i)
                    {
                        if (size >= SIZE_FACTORS[i])
                        {
                            long value = (size * 10) / SIZE_FACTORS[i];
                            return (value / 10).ToString() + '.' +
                                    ((int)(value % 10)).ToString() +
                                    SIZE_UNITS[i];
                        }
                    }
                }
            }

            private String FormatDate(DateTime date)
            {
                if (date == null)
                {
                    return "";
                }
                return date.ToString(this._dateFormat);
            }
        }

        class StateSnapshot
        {
            protected internal Entry _leadEntry;
            protected internal Entry _anchorEntry;
            protected internal Entry[] _selected;

            protected internal StateSnapshot(Entry leadEntry, Entry anchorEntry, Entry[] selected)
            {
                this._leadEntry = leadEntry;
                this._anchorEntry = anchorEntry;
                this._selected = selected;
            }
        }

        class NameComparator : IComparer<Entry>
        {
            protected internal static NameComparator instance = new NameComparator();
            public int Compare(Entry o1, Entry o2)
            {
                return Comparer<string>.Default.Compare(o1.Name, o2.Name);
            }
        }

        class ExtensionComparator : IComparer<Entry>
        {
            protected internal static ExtensionComparator instance = new ExtensionComparator();
            public int Compare(Entry o1, Entry o2)
            {
                return Comparer<string>.Default.Compare(o1.GetExtension(), o2.GetExtension());
            }
        }

        class SizeComparator : IComparer<Entry>
        {
            protected internal static SizeComparator instance = new SizeComparator();
            public int Compare(Entry o1, Entry o2)
            {
                return Math.Sign(o1.Size - o2.Size);
            }
        }

        class LastModifiedComparator : IComparer<Entry>
        {
            protected internal static LastModifiedComparator instance = new LastModifiedComparator();
            public int Compare(Entry o1, Entry o2)
            {
                DateTime lm1 = o1.lastModified;
                DateTime lm2 = o2.lastModified;
                if (lm1 != null && lm2 != null)
                {
                    return lm1.CompareTo(lm2);
                }
                if (lm1 != null)
                {
                    return 1;
                }
                if (lm2 != null)
                {
                    return -1;
                }
                return 0;
            }
        }

        private class FileFilterWrapper : FileFilter
        {
            private FileFilter _baseFilter;
            private bool _showFolder;
            private bool _showHidden;

            public FileFilterWrapper(FileFilter baseFilter, bool showFolder, bool showHidden)
            {
                this._baseFilter = baseFilter;
                this._showFolder = showFolder;
                this._showHidden = showHidden;
            }

            public bool Accept(Object file)
            {
                if (_showHidden || !((IO.FileSystemObject)file).IsHidden)
                {
                    if (((IO.FileSystemObject)file).IsDirectory)
                    {
                        return _showFolder;
                    }
                    return (_baseFilter == null) || _baseFilter.Accept(file);
                }
                return false;
            }
        }
    }

    public class FileTableSortingChangedEventArgs
    {
    }

    public class FileTableSelectionChangedEventArgs
    {
    }
}
