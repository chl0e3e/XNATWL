using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class FileTable : Table
    {
        public enum SortColumn
        {
            NAME,
            TYPE,
            SIZE,
            LAST_MODIFIED
        }

        public static IComparer<Entry> SortColumn_Comparer(SortColumn sortColumn)
        {
            switch(sortColumn)
            {
                case SortColumn.NAME:
                    return NameComparator.instance;
                case SortColumn.TYPE:
                    return ExtensionComparator.instance;
                case SortColumn.SIZE:
                    return SizeComparator.instance;
                case SortColumn.LAST_MODIFIED:
                    return LastModifiedComparator.instance;
                default:
                    return NameComparator.instance;
            }
        }

        private FileTableModel fileTableModel;
        private TableSelectionModel fileTableSelectionModel;
        private TableSearchWindow tableSearchWindow;
        private SortColumn sortColumn = SortColumn.NAME;
        private SortOrder sortOrder = SortOrder.ASCENDING;

        private bool allowMultiSelection;
        private FileFilter fileFilter = null;
        private bool showFolders = true;
        private bool showHidden = false;

        private FileSystemModel fsm;
        private Object currentFolder;

        public event EventHandler<FileTableSelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<FileTableSortingChangedEventArgs> SortingChanged;

        public FileTable()
        {
            fileTableModel = new FileTableModel(this);
            setModel(fileTableModel);

            /*selectionChangedListener = new Runnable() {
                public void run() {
                    selectionChanged();
                }
            };*/
        }

        public bool getShowFolders()
        {
            return showFolders;
        }

        public void setShowFolders(bool showFolders)
        {
            if (this.showFolders != showFolders)
            {
                this.showFolders = showFolders;
                refreshFileTable();
            }
        }

        public bool getShowHidden()
        {
            return showHidden;
        }

        public void setShowHidden(bool showHidden)
        {
            if (this.showHidden != showHidden)
            {
                this.showHidden = showHidden;
                refreshFileTable();
            }
        }

        public void setFileFilter(FileFilter filter)
        {
            // always refresh, filter parameters could have been changed
            fileFilter = filter;
            refreshFileTable();
        }

        public FileFilter getFileFilter()
        {
            return fileFilter;
        }

        public Entry[] getSelection()
        {
            return fileTableModel.getEntries(fileTableSelectionModel.Selection);
        }

        public void setSelection(params Object[] files)
        {
            fileTableSelectionModel.ClearSelection();
            foreach (Object file in files)
            {
                int idx = fileTableModel.findFile(file);
                if (idx >= 0)
                {
                    fileTableSelectionModel.AddSelection(idx, idx);
                }
            }
        }

        public bool setSelection(Object file)
        {
            fileTableSelectionModel.ClearSelection();
            int idx = fileTableModel.findFile(file);
            if (idx >= 0)
            {
                fileTableSelectionModel.AddSelection(idx, idx);
                scrollToRow(idx);
                return true;
            }
            return false;
        }

        public void clearSelection()
        {
            fileTableSelectionModel.ClearSelection();
        }

        public void setSortColumn(SortColumn column)
        {
            if (sortColumn != column)
            {
                sortColumn = column;
                sortingChanged();
            }
        }

        public void setSortOrder(SortOrder order)
        {
            if (sortOrder != order)
            {
                sortOrder = order;
                sortingChanged();
            }
        }

        public bool getAllowMultiSelection()
        {
            return allowMultiSelection;
        }

        public void setAllowMultiSelection(bool allowMultiSelection)
        {
            this.allowMultiSelection = allowMultiSelection;
            if (fileTableSelectionModel != null)
            {
                fileTableSelectionModel.SelectionChanged -= FileTableSelectionModel_SelectionChanged;
            }
            if (tableSearchWindow != null)
            {
                tableSearchWindow.setModel(null, 0);
            }
            if (allowMultiSelection)
            {
                fileTableSelectionModel = new DefaultTableSelectionModel();
            }
            else
            {
                fileTableSelectionModel = new TableSingleSelectionModel();
            }
            fileTableSelectionModel.SelectionChanged += FileTableSelectionModel_SelectionChanged;
            tableSearchWindow = new TableSearchWindow(this, fileTableSelectionModel);
            tableSearchWindow.setModel(fileTableModel, 0);
            setSelectionManager(new TableRowSelectionManager(fileTableSelectionModel));
            setKeyboardSearchHandler(tableSearchWindow);
            selectionChanged();
        }

        private void FileTableSelectionModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectionChanged();
        }

        public FileSystemModel getFileSystemModel()
        {
            return fsm;
        }

        public Object getCurrentFolder()
        {
            return currentFolder;
        }

        public bool isRoot()
        {
            return currentFolder == null;
        }

        public void setCurrentFolder(FileSystemModel fsm, Object folder)
        {
            this.fsm = fsm;
            this.currentFolder = folder;
            refreshFileTable();
        }

        public void refreshFileTable()
        {
            Object[] objs = collectObjects();
            if (objs != null)
            {
                int lastFileIdx = objs.Length;
                Entry[] entries = new Entry[lastFileIdx];
                int numFolders = 0;
                bool bIsRoot = isRoot();
                for (int i = 0; i < objs.Length; i++)
                {
                    Entry e = new Entry(fsm, objs[i], bIsRoot);
                    if (e.isFolder)
                    {
                        entries[numFolders++] = e;
                    }
                    else
                    {
                        entries[--lastFileIdx] = e;
                    }
                }
                Array.Sort(entries, 0, numFolders, NameComparator.instance);
                sortFilesAndUpdateModel(entries, numFolders);
            }
            else
            {
                sortFilesAndUpdateModel(EMPTY, 0);
            }
            if (tableSearchWindow != null)
            {
                tableSearchWindow.cancelSearch();
            }
        }

        protected void selectionChanged()
        {
            this.SelectionChanged.Invoke(this, new FileTableSelectionChangedEventArgs());
        }

        protected void sortingChanged()
        {
            setSortArrows();
            sortFilesAndUpdateModel();
            this.SortingChanged.Invoke(this, new FileTableSortingChangedEventArgs());
        }

        private Object[] collectObjects()
        {
            if (fsm == null)
            {
                return null;
            }
            if (isRoot())
            {
                return fsm.ListRoots();
            }
            FileFilter filter = fileFilter;
            if (filter != null || !getShowFolders() || !getShowHidden())
            {
                filter = new FileFilterWrapper(filter, getShowFolders(), getShowHidden());
            }
            return fsm.ListFolder(currentFolder, filter);
        }

        private void sortFilesAndUpdateModel(Entry[] entries, int numFolders)
        {
            StateSnapshot snapshot = makeSnapshot();
            Array.Sort(entries, numFolders, entries.Length, SortColumn_Comparer(sortColumn));
            fileTableModel.setData(entries, numFolders);
            restoreSnapshot(snapshot);
        }

        protected override void columnHeaderClicked(int column)
        {
            base.columnHeaderClicked(column);

            SortColumn thisColumn = (SortColumn) Enum.GetValues(typeof(SortColumn)).GetValue(column);
            if (sortColumn == thisColumn)
            {
                setSortOrder(SortOrderStatics.SortOrder_Invert(sortOrder));
            }
            else
            {
                setSortColumn(thisColumn);
            }
        }

        protected override void updateColumnHeaderNumbers()
        {
            base.updateColumnHeaderNumbers();
            setSortArrows();
        }

        protected void setSortArrows()
        {
            int i = 0;
            foreach (SortColumn column in Enum.GetValues(typeof(SortColumn)))
            {
                if (column == sortColumn)
                {
                    break;
                }
                i++;
            }
            setColumnSortOrderAnimationState(i, sortOrder);
        }

        private void sortFilesAndUpdateModel()
        {
            sortFilesAndUpdateModel(fileTableModel.entries, fileTableModel.numFolders);
        }

        private StateSnapshot makeSnapshot()
        {
            return new StateSnapshot(
                    fileTableModel.getEntry(fileTableSelectionModel.LeadIndex),
                    fileTableModel.getEntry(fileTableSelectionModel.AnchorIndex),
                    fileTableModel.getEntries(fileTableSelectionModel.Selection));
        }

        private void restoreSnapshot(StateSnapshot snapshot)
        {
            foreach (Entry e in snapshot.selected)
            {
                int idx = fileTableModel.findEntry(e);
                if (idx >= 0)
                {
                    fileTableSelectionModel.AddSelection(idx, idx);
                }
            }
            int leadIndex = fileTableModel.findEntry(snapshot.leadEntry);
            int anchorIndex = fileTableModel.findEntry(snapshot.anchorEntry);
            fileTableSelectionModel.LeadIndex = leadIndex;
            fileTableSelectionModel.AnchorIndex = anchorIndex;
            scrollToRow(Math.Max(0, leadIndex));
        }

        static Entry[] EMPTY = new Entry[0];

        public class Entry
        {
            public FileSystemModel fsm;
            public Object obj;
            public String name;
            public bool isFolder;
            public long size;
            /** last modified date - can be null */
            public DateTime lastModified;

            public Entry(FileSystemModel fsm, Object obj, bool isRoot)
            {
                this.fsm = fsm;
                this.obj = obj;
                this.name = fsm.NameOf(obj);
                if (isRoot)
                {
                    // don't call getLastModified on roots - causes bad performance
                    // on windows when a DVD/CD/Floppy has no media inside
                    this.isFolder = true;
                    this.lastModified = DateTime.MinValue;
                }
                else
                {
                    this.isFolder = fsm.IsFolder(obj);
                    this.lastModified = new DateTime(fsm.LastModifiedOf(obj));
                }
                if (isFolder)
                {
                    this.size = 0;
                }
                else
                {
                    this.size = fsm.SizeOf(obj);
                }
            }

            public String getExtension()
            {
                int idx = name.LastIndexOf('.');
                if (idx >= 0)
                {
                    return name.Substring(idx + 1);
                }
                else
                {
                    return "";
                }
            }

            public String getPath()
            {
                return fsm.PathOf(obj);
            }

            public override bool Equals(Object o)
            {
                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }
                Entry that = (Entry)o;
                return (this.fsm == that.fsm) && fsm.Equals(this.obj, that.obj);
            }

            public override int GetHashCode()
            {
                return (obj != null) ? obj.GetHashCode() : 203;
            }
        }

        class FileTableModel : AbstractTableModel
        {
            //private DateFormat dateFormat = DateFormat.getDateInstance();
            private string dateFormat = DateTimeFormatInfo.CurrentInfo.FullDateTimePattern;

            protected internal Entry[] entries = EMPTY;
            protected internal int numFolders;

            private FileTable fileTable;

            public FileTableModel(FileTable fileTable)
            {
                this.fileTable = fileTable;
            }

            public void setData(Entry[] entries, int numFolders)
            {
                this.FireRowsDeleted(0, this.Rows);
                this.entries = entries;
                this.numFolders = numFolders;
                this.FireRowsInserted(0, this.Rows);
            }

            static String[] COLUMN_HEADER = { "File name", "Type", "Size", "Last modified" };

            public override String ColumnHeaderTextFor(int column)
            {
                return COLUMN_HEADER[column];
            }

            public override Object CellAt(int row, int column)
            {
                Entry e = entries[row];
                if (e.isFolder)
                {
                    switch (column)
                    {
                        case 0: return "[" + e.name + "]";
                        case 1: return "Folder";
                        case 2: return "";
                        case 3: return formatDate(e.lastModified);
                        default: return "??";
                    }
                }
                else
                {
                    switch (column)
                    {
                        case 0: return e.name;
                        case 1:
                            {
                                String ext = e.getExtension();
                                return (ext.Length == 0) ? "File" : ext + "-file";
                            }
                        case 2: return formatFileSize(e.size);
                        case 3: return formatDate(e.lastModified);
                        default: return "??";
                    }
                }
            }

            public override Object TooltipAt(int row, int column)
            {
                Entry e = entries[row];
                StringBuilder sb = new StringBuilder(e.name);
                if (!e.isFolder)
                {
                    sb.Append("\nSize: ").Append(formatFileSize(e.size));
                }
                if (e.lastModified != null)
                {
                    sb.Append("\nLast modified: ").Append(formatDate(e.lastModified));
                }
                return sb.ToString();
            }

            protected internal Entry getEntry(int row)
            {
                if (row >= 0 && row < entries.Length)
                {
                    return entries[row];
                }
                else
                {
                    return null;
                }
            }

            protected internal int findEntry(Entry entry)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Equals(entry))
                    {
                        return i;
                    }
                }
                return -1;
            }

            protected internal int findFile(Object file)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    Entry e = entries[i];
                    if (e.fsm.Equals(e.obj, file))
                    {
                        return i;
                    }
                }
                return -1;
            }

            protected internal Entry[] getEntries(int[] selection)
            {
                int count = selection.Length;
                if (count == 0)
                {
                    return EMPTY;
                }
                Entry[] result = new Entry[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = entries[selection[i]];
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
                    return entries.Length;
                }
            }

            private String formatFileSize(long size)
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

            private String formatDate(DateTime date)
            {
                if (date == null)
                {
                    return "";
                }
                return date.ToString(this.dateFormat);
            }
        }

        class StateSnapshot
        {
            protected internal Entry leadEntry;
            protected internal Entry anchorEntry;
            protected internal Entry[] selected;

            protected internal StateSnapshot(Entry leadEntry, Entry anchorEntry, Entry[] selected)
            {
                this.leadEntry = leadEntry;
                this.anchorEntry = anchorEntry;
                this.selected = selected;
            }
        }

        class NameComparator : IComparer<Entry>
        {
            protected internal static NameComparator instance = new NameComparator();
            public int Compare(Entry o1, Entry o2)
            {
                return Comparer<string>.Default.Compare(o1.name, o2.name);
            }
        }

        class ExtensionComparator : IComparer<Entry>
        {
            protected internal static ExtensionComparator instance = new ExtensionComparator();
            public int Compare(Entry o1, Entry o2)
            {
                return Comparer<string>.Default.Compare(o1.getExtension(), o2.getExtension());
            }
        }

        class SizeComparator : IComparer<Entry>
        {
            protected internal static SizeComparator instance = new SizeComparator();
            public int Compare(Entry o1, Entry o2)
            {
                return Math.Sign(o1.size - o2.size);
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
            private FileFilter baseFilter;
            private bool showFolder;
            private bool showHidden;
            public FileFilterWrapper(FileFilter baseFilter, bool showFolder, bool showHidden)
            {
                this.baseFilter = baseFilter;
                this.showFolder = showFolder;
                this.showHidden = showHidden;
            }
            public bool Accept(Object file)
            {
                if (showHidden || !((IO.FileSystemObject)file).IsHidden)
                {
                    if (((IO.FileSystemObject)file).IsDirectory)
                    {
                        return showFolder;
                    }
                    return (baseFilter == null) || baseFilter.Accept(file);
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
