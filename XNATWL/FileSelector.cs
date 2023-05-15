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
using XNATWL.IO;
using XNATWL.Model;

namespace XNATWL
{
    public class FileSelector : DialogLayout
    {
        public event EventHandler<FileSelectorFilesSelectedEventArgs> FilesSelected;
        public event EventHandler<FileSelectorCancelledEventArgs> Cancelled;
        public event EventHandler<FileSelectorFolderChangedEventArgs> FolderChanged;
        public event EventHandler<FileSelectorSelectionChangedEventArgs> SelectionChanged;

        public class NamedFileFilter
        {
            private String name;
            private FileFilter fileFilter;

            public NamedFileFilter(String name, FileFilter fileFilter)
            {
                this.name = name;
                this.fileFilter = fileFilter;
            }
            public String getDisplayName()
            {
                return name;
            }
            public FileFilter getFileFilter()
            {
                return fileFilter;
            }
        }

        public static NamedFileFilter AllFilesFilter = new NamedFileFilter("All files", null);

        private IntegerModel _flags;
        private MRUListModel<String> _folderMRU;
        MRUListModel<String> _filesMRU;

        private TreeComboBox _currentFolder;
        private Label _labelCurrentFolder;
        private FileTable _fileTable;
        private ScrollPane _fileTableSP;
        private Button _btnUp;
        private Button _btnHome;
        private Button _btnFolderMRU;
        private Button _btnFilesMRU;
        private Button _btnOk;
        private Button _btnCancel;
        private Button _btnRefresh;
        private Button _btnShowFolders;
        private Button _btnShowHidden;
        private ComboBox<String> _fileFilterBox;
        private FileFiltersModel _fileFiltersModel;
        private EditFieldAutoCompletionWindow _autoCompletion;

        private bool _allowFolderSelection;
        private NamedFileFilter _activeFileFilter;

        FileSystemModel _fileSystemModel;
        private FileSystemTreeModel _model;

        private Widget _userWidgetBottom;
        private Widget _userWidgetRight;

        private Object _fileToSelectOnSetCurrentNode;

        /**
         * Create a FileSelector without persistent state
         */
        public FileSelector() : this(null, null)
        {
        }

        class FileSelectorPathResolver : TreeComboBox.PathResolver
        {
            private FileSelector _fileSelector;

            public FileSelectorPathResolver(FileSelector fileSelector)
            {
                this._fileSelector = fileSelector;
            }

            public TreeTableNode ResolvePath(TreeTableModel model, string path)
            {
                return this._fileSelector.ResolvePath(path);
            }
        }

        public FileSelector(Preferences prefs, String prefsKey)
        {
            if ((prefs == null) != (prefsKey == null))
            {
                throw new ArgumentNullException("'prefs' and 'prefsKey' must both be valid or both null");
            }

            if (prefs != null)
            {
                _flags = new PersistentIntegerModel(prefs, prefsKey + "_Flags", 0, 0xFFFF, 0);
                _folderMRU = new PersistentMRUListModel<String>(10, typeof(String), prefs, prefsKey + "_foldersMRU");
                _filesMRU = new PersistentMRUListModel<String>(20, typeof(String), prefs, prefsKey + "_filesMRU");
            }
            else
            {
                _flags = new SimpleIntegerModel(0, 0xFFFF, 0);
                _folderMRU = new SimpleMRUListModel<String>(10);
                _filesMRU = new SimpleMRUListModel<String>(20);
            }

            _currentFolder = new TreeComboBox();
            _currentFolder.SetTheme("currentFolder");
            _fileTable = new FileTable();
            _fileTable.SetTheme("fileTable");
            _fileTable.SelectionChanged += (sender, e) =>
            {
                this.SetAndFireSelectionChanged();
            };

            _btnUp = new Button();
            _btnUp.SetTheme("buttonUp");
            _btnUp.Action += (sender, e) =>
            {
                GoOneLevelUp();
            };

            _btnHome = new Button();
            _btnHome.SetTheme("buttonHome");
            _btnHome.Action += (sender, e) =>
            {
                GoHome();
            };

            _btnFolderMRU = new Button();
            _btnFolderMRU.SetTheme("buttonFoldersMRU");
            _btnFolderMRU.Action += (sender, e) =>
            {
                ShowFolderMRU();
            };

            _btnFilesMRU = new Button();
            _btnFilesMRU.SetTheme("buttonFilesMRU");

            _btnFilesMRU.Action += (sender, e) =>
            {
                ShowFilesMRU();
            };

            _btnOk = new Button();
            _btnOk.SetTheme("buttonOk");
            _btnOk.Action += (sender, e) =>
            {
                AcceptSelection();
            };

            _btnCancel = new Button();
            _btnCancel.SetTheme("buttonCancel");
            _btnCancel.Action += (sender, e) =>
            {
                FireCancelled();
            };

            _currentFolder.SetPathResolver(new FileSelectorPathResolver(this));
            _currentFolder.SelectedNodeChanged += (sender, e) =>
            {
                SetCurrentNode(e.Node, e.PreviousChildNode);
            };

            _autoCompletion = new EditFieldAutoCompletionWindow(_currentFolder.GetEditField());
            //autoCompletion.setUseInvokeAsync(true);
            _currentFolder.GetEditField().SetAutoCompletionWindow(_autoCompletion);

            _fileTable.SetAllowMultiSelection(true);
            _fileTable.DoubleClick += (sender, e) =>
            {
                this.AcceptSelection();
            };

            _activeFileFilter = AllFilesFilter;
            _fileFiltersModel = new FileFiltersModel();
            _fileFilterBox = new ComboBox<String>(_fileFiltersModel);
            _fileFilterBox.SetTheme("fileFiltersBox");
            _fileFilterBox.SetComputeWidthFromModel(true);
            _fileFilterBox.SetVisible(false);
            _fileFilterBox.SelectionChanged += (sender, e) =>
            {
                FileFilterChanged();
            };

            _labelCurrentFolder = new Label("Folder");
            _labelCurrentFolder.SetLabelFor(_currentFolder);

            _fileTableSP = new ScrollPane(_fileTable);


            _btnRefresh = new Button();
            _btnRefresh.SetTheme("buttonRefresh");
            _btnRefresh.Action += BtnRefresh_Action;

            _btnShowFolders = new Button(new ToggleButtonModel(new BitFieldBooleanModel(_flags, 0), true));
            _btnShowFolders.SetTheme("buttonShowFolders");
            _btnShowFolders.Action += BtnRefresh_Action;

            _btnShowHidden = new Button(new ToggleButtonModel(new BitFieldBooleanModel(_flags, 1), false));
            _btnShowHidden.SetTheme("buttonShowHidden");
            _btnShowHidden.Action += BtnRefresh_Action;

            AddActionMapping("goOneLevelUp", "GoOneLevelUp");
            AddActionMapping("acceptSelection", "AcceptSelection");
        }

        private void BtnRefresh_Action(object sender, ButtonActionEventArgs e)
        {
            this.RefreshFileTable();
        }

        protected void CreateLayout()
        {
            SetHorizontalGroup(null);
            SetVerticalGroup(null);
            RemoveAllChildren();

            Add(_fileTableSP);
            Add(_fileFilterBox);
            Add(_btnOk);
            Add(_btnCancel);
            Add(_btnRefresh);
            Add(_btnShowFolders);
            Add(_btnShowHidden);
            Add(_labelCurrentFolder);
            Add(_currentFolder);
            Add(_btnFolderMRU);
            Add(_btnUp);

            Group hCurrentFolder = CreateSequentialGroup()
                    .AddWidget(_labelCurrentFolder)
                    .AddWidget(_currentFolder)
                    .AddWidget(_btnFolderMRU)
                    .AddWidget(_btnUp)
                    .AddWidget(_btnHome);
            Group vCurrentFolder = CreateParallelGroup()
                    .AddWidget(_labelCurrentFolder)
                    .AddWidget(_currentFolder)
                    .AddWidget(_btnFolderMRU)
                    .AddWidget(_btnUp)
                    .AddWidget(_btnHome);

            Group hButtonGroup = CreateSequentialGroup()
                    .AddWidget(_btnRefresh)
                    .AddGap(MEDIUM_GAP)
                    .AddWidget(_btnShowFolders)
                    .AddWidget(_btnShowHidden)
                    .AddWidget(_fileFilterBox)
                    .AddGap("buttonBarLeft")
                    .AddWidget(_btnFilesMRU)
                    .AddGap("buttonBarSpacer")
                    .AddWidget(_btnOk)
                    .AddGap("buttonBarSpacer")
                    .AddWidget(_btnCancel)
                    .AddGap("buttonBarRight");
            Group vButtonGroup = CreateParallelGroup()
                    .AddWidget(_btnRefresh)
                    .AddWidget(_btnShowFolders)
                    .AddWidget(_btnShowHidden)
                    .AddWidget(_fileFilterBox)
                    .AddWidget(_btnFilesMRU)
                    .AddWidget(_btnOk)
                    .AddWidget(_btnCancel);

            Group horz = CreateParallelGroup()
                    .AddGroup(hCurrentFolder)
                    .AddWidget(_fileTableSP);

            Group vert = CreateSequentialGroup()
                    .AddGroup(vCurrentFolder)
                    .AddWidget(_fileTableSP);

            if (_userWidgetBottom != null)
            {
                horz.AddWidget(_userWidgetBottom);
                vert.AddWidget(_userWidgetBottom);
            }

            if (_userWidgetRight != null)
            {
                horz = CreateParallelGroup().AddGroup(CreateSequentialGroup()
                        .AddGroup(horz)
                        .AddWidget(_userWidgetRight));
                vert = CreateSequentialGroup().AddGroup(CreateParallelGroup()
                        .AddGroup(vert)
                        .AddWidget(_userWidgetRight));
            }

            SetHorizontalGroup(horz.AddGroup(hButtonGroup));
            SetVerticalGroup(vert.AddGroup(vButtonGroup));
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            CreateLayout();
        }

        public FileSystemModel GetFileSystemModel()
        {
            return _fileSystemModel;
        }

        public void SetFileSystemModel(FileSystemModel fsm)
        {
            this._fileSystemModel = fsm;
            if (fsm == null)
            {
                _model = null;
                _currentFolder.SetModel(null);
                _fileTable.SetCurrentFolder(null, null);
                _autoCompletion.SetDataSource(null);
            }
            else
            {
                _model = new FileSystemTreeModel(fsm);
                _model.SetSorter(new NameSorter(fsm));
                _currentFolder.SetModel(_model);
                _currentFolder.SetSeparator(fsm.Separator);
                _autoCompletion.SetDataSource(new FileSystemAutoCompletionDataSource(fsm,
                        FolderFilter.Instance));
                if (!GotoFolderFromMRU(0) && !GoHome())
                {
                    SetCurrentNode(_model);
                }
            }
        }

        public bool GetAllowMultiSelection()
        {
            return _fileTable.GetAllowMultiSelection();
        }

        /**
         * Controls if multi selection is allowed.
         *
         * Default is true.
         *
         * @param allowMultiSelection true if multiple files can be selected.
         */
        public void SetAllowMultiSelection(bool allowMultiSelection)
        {
            _fileTable.SetAllowMultiSelection(allowMultiSelection);
        }

        public bool GetAllowFolderSelection()
        {
            return _allowFolderSelection;
        }

        /**
         * Controls if folders can be selected. If false then the "Ok" button
         * is disabled when a folder is selected.
         *
         * Default is false.
         *
         * @param allowFolderSelection true if folders can be selected
         */
        public void SetAllowFolderSelection(bool allowFolderSelection)
        {
            this._allowFolderSelection = allowFolderSelection;
            SetAndFireSelectionChanged();
        }

        public bool GetAllowHorizontalScrolling()
        {
            return _fileTableSP.GetFixed() != ScrollPane.Fixed.HORIZONTAL;
        }

        /**
         * Controls if the file table allows horizontal scrolling or not.
         * 
         * Default is true.
         * 
         * @param allowHorizontalScrolling true if horizontal scrolling is allowed
         */
        public void SetAllowHorizontalScrolling(bool allowHorizontalScrolling)
        {
            _fileTableSP.SetFixed(allowHorizontalScrolling
                    ? ScrollPane.Fixed.NONE
                    : ScrollPane.Fixed.HORIZONTAL);
        }

        public Widget GetUserWidgetBottom()
        {
            return _userWidgetBottom;
        }

        public void SetUserWidgetBottom(Widget userWidgetBottom)
        {
            this._userWidgetBottom = userWidgetBottom;
            CreateLayout();
        }

        public Widget GetUserWidgetRight()
        {
            return _userWidgetRight;
        }

        public void SetUserWidgetRight(Widget userWidgetRight)
        {
            this._userWidgetRight = userWidgetRight;
            CreateLayout();
        }

        public FileTable GetFileTable()
        {
            return _fileTable;
        }

        public void SetOkButtonEnabled(bool enabled)
        {
            _btnOk.SetEnabled(enabled);
        }

        public Object GetCurrentFolder()
        {
            Object node = _currentFolder.GetCurrentNode();
            if (node is FolderNode)
            {
                return ((FolderNode)node).Folder;
            }
            else
            {
                return null;
            }
        }

        public bool SetCurrentFolder(Object folder)
        {
            FolderNode node = _model.NodeForFolder(folder);
            if (node != null)
            {
                SetCurrentNode(node);
                return true;
            }
            return false;
        }

        public bool SelectFile(Object file)
        {
            if (_fileSystemModel == null)
            {
                return false;
            }
            Object parent = _fileSystemModel.Parent(file);
            if (SetCurrentFolder(parent))
            {
                return _fileTable.SetSelection(file);
            }
            return false;
        }

        public void ClearSelection()
        {
            _fileTable.ClearSelection();
        }

        /**
         * Adds a named file filter to the FileSelector.
         *
         * The first added file filter is selected as default.
         *
         * @param filter the file filter.
         * @throws NullPointerException if filter is null
         * @see #AllFilesFilter
         */
        public void AddFileFilter(NamedFileFilter filter)
        {
            if (filter == null)
            {
                throw new NullReferenceException("filter");
            }
            _fileFiltersModel.AddFileFilter(filter);
            _fileFilterBox.SetVisible(_fileFiltersModel.GetNumEntries() > 0);
            if (_fileFilterBox.GetSelected() < 0)
            {
                _fileFilterBox.SetSelected(0);
            }
        }

        public void RemoveFileFilter(NamedFileFilter filter)
        {
            if (filter == null)
            {
                throw new NullReferenceException("filter");
            }
            _fileFiltersModel.RemoveFileFilter(filter);
            if (_fileFiltersModel.GetNumEntries() == 0)
            {
                _fileFilterBox.SetVisible(false);
                SetFileFilter(AllFilesFilter);
            }
        }

        public void RemoveAllFileFilters()
        {
            _fileFiltersModel.RemoveAll();
            _fileFilterBox.SetVisible(false);
            SetFileFilter(AllFilesFilter);
        }

        public void SetFileFilter(NamedFileFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }
            int idx = _fileFiltersModel.FindFilter(filter);
            if (idx < 0)
            {
                throw new ArgumentOutOfRangeException("filter not registered");
            }
            _fileFilterBox.SetSelected(idx);
        }

        public NamedFileFilter GetFileFilter()
        {
            return _activeFileFilter;
        }

        public bool GetShowFolders()
        {
            return _btnShowFolders.GetModel().Selected;
        }

        public void SetShowFolders(bool showFolders)
        {
            _btnShowFolders.GetModel().Selected = showFolders;
        }

        public bool GetShowHidden()
        {
            return _btnShowHidden.GetModel().Selected;
        }

        public void SetShowHidden(bool showHidden)
        {
            _btnShowHidden.GetModel().Selected = showHidden;
        }

        public void GoOneLevelUp()
        {
            TreeTableNode node = _currentFolder.GetCurrentNode();
            TreeTableNode parent = node.Parent;
            if (parent != null)
            {
                SetCurrentNode(parent, node);
            }
        }

        public bool GoHome()
        {
            if (_fileSystemModel != null)
            {
                Object folder = _fileSystemModel.SpecialFolder(DotNetFileSystemModel.USERPROFILE_FOLDER);
                if (folder != null)
                {
                    return SetCurrentFolder(folder);
                }
            }
            return false;
        }

        public void AcceptSelection()
        {
            FileTable.Entry[] selection = _fileTable.GetSelection();
            if (selection.Length == 1)
            {
                FileTable.Entry entry = selection[0];
                if (entry != null && entry.IsFolder)
                {
                    SetCurrentFolder(entry.Obj);
                    return;
                }
            }
            FireAcceptCallback(selection);
        }

        void FileFilterChanged()
        {
            int idx = _fileFilterBox.GetSelected();
            if (idx >= 0)
            {
                NamedFileFilter filter = _fileFiltersModel.GetFileFilter(idx);
                _activeFileFilter = filter;
                _fileTable.SetFileFilter(filter.getFileFilter());
            }
        }

        void FireAcceptCallback(FileTable.Entry[] selection)
        {
            if (this.FilesSelected != null)
            {
                Object[] objects = new Object[selection.Length];
                for (int i = 0; i < selection.Length; i++)
                {
                    FileTable.Entry e = selection[i];
                    if (e.IsFolder && !_allowFolderSelection)
                    {
                        return;
                    }
                    objects[i] = e.Obj;
                }
                AddToMRU(selection);
                this.FilesSelected.Invoke(this, new FileSelectorFilesSelectedEventArgs(objects));
            }
        }

        void FireCancelled()
        {
            if (this.Cancelled != null)
            {
                this.Cancelled.Invoke(this, new FileSelectorCancelledEventArgs());
            }
        }

        void SetAndFireSelectionChanged()
        {
            bool foldersSelected = false;
            bool filesSelected = false;
            FileTable.Entry[] selection = _fileTable.GetSelection();
            foreach (FileTable.Entry entry in selection)
            {
                if (entry.IsFolder)
                {
                    foldersSelected = true;
                }
                else
                {
                    filesSelected = true;
                }
            }
            if (_allowFolderSelection)
            {
                _btnOk.SetEnabled(filesSelected || foldersSelected);
            }
            else
            {
                _btnOk.SetEnabled(filesSelected && !foldersSelected);
            }

            if (this.SelectionChanged != null)
            {
                this.SelectionChanged.Invoke(this, new FileSelectorSelectionChangedEventArgs(selection));
            }
        }

        protected void SetCurrentNode(TreeTableNode node, TreeTableNode childToSelect)
        {
            if (childToSelect is FolderNode)
            {
                _fileToSelectOnSetCurrentNode = ((FolderNode)childToSelect).Folder;
            }
            SetCurrentNode(node);
        }

        protected void SetCurrentNode(TreeTableNode node)
        {
            _currentFolder.SetCurrentNode(node);
            RefreshFileTable();
            if (this.FolderChanged != null)
            {
                Object curFolder = GetCurrentFolder();
                this.FolderChanged.Invoke(this, new FileSelectorFolderChangedEventArgs(curFolder));
            }
            if (_fileToSelectOnSetCurrentNode != null)
            {
                _fileTable.SetSelection(_fileToSelectOnSetCurrentNode);
                _fileToSelectOnSetCurrentNode = null;
            }
        }

        void RefreshFileTable()
        {
            _fileTable.SetShowFolders(_btnShowFolders.GetModel().Selected);
            _fileTable.SetShowHidden(_btnShowHidden.GetModel().Selected);
            _fileTable.SetCurrentFolder(_fileSystemModel, GetCurrentFolder());
        }

        TreeTableNode ResolvePath(String path)
        {
            Object obj = _fileSystemModel.FileByPath(path);
            _fileToSelectOnSetCurrentNode = null;
            if (obj != null)
            {
                if (_fileSystemModel.IsFile(obj))
                {
                    _fileToSelectOnSetCurrentNode = obj;
                    obj = _fileSystemModel.Parent(obj);
                }
                FolderNode node = _model.NodeForFolder(obj);
                if (node != null)
                {
                    return node;
                }
            }
            throw new ArgumentException("Could not resolve: " + path);
        }

        void ShowFolderMRU()
        {
            PopupWindow popup = new PopupWindow(this);
            ListBox<String> listBox = new ListBox<String>(_folderMRU);
            popup.SetTheme("fileselector-folderMRUpopup");
            popup.Add(listBox);
            if (popup.OpenPopup())
            {
                popup.SetInnerSize(GetInnerWidth() * 2 / 3, GetInnerHeight() * 2 / 3);
                popup.SetPosition(_btnFolderMRU.GetX() - popup.GetWidth(), _btnFolderMRU.GetY());
                listBox.Callback += (sender, e) =>
                {
                    if (ListBox<String>.CallbackReason_ActionRequested(e.Reason))
                    {
                        popup.ClosePopup();
                        int idx = listBox.GetSelected();
                        if (idx >= 0)
                        {
                            GotoFolderFromMRU(idx);
                        }
                    }
                };
            }
        }

        void ShowFilesMRU()
        {
            PopupWindow popup = new PopupWindow(this);
            DialogLayout layout = new DialogLayout();
            ListBox<String> listBox = new ListBox<String>(_filesMRU);
            Button popupBtnOk = new Button();
            Button popupBtnCancel = new Button();
            popupBtnOk.SetTheme("buttonOk");
            popupBtnCancel.SetTheme("buttonCancel");
            popup.SetTheme("fileselector-filesMRUpopup");
            popup.Add(layout);
            layout.Add(listBox);
            layout.Add(popupBtnOk);
            layout.Add(popupBtnCancel);

            DialogLayout.Group hBtnGroup = layout.CreateSequentialGroup()
                    .AddGap().AddWidget(popupBtnOk).AddWidget(popupBtnCancel);
            DialogLayout.Group vBtnGroup = layout.CreateParallelGroup()
                    .AddWidget(popupBtnOk).AddWidget(popupBtnCancel);
            layout.SetHorizontalGroup(layout.CreateParallelGroup().AddWidget(listBox).AddGroup(hBtnGroup));
            layout.SetVerticalGroup(layout.CreateSequentialGroup().AddWidget(listBox).AddGroup(vBtnGroup));

            if (popup.OpenPopup())
            {
                popup.SetInnerSize(GetInnerWidth() * 2 / 3, GetInnerHeight() * 2 / 3);
                popup.SetPosition(GetInnerX() + (GetInnerWidth() - popup.GetWidth()) / 2, _btnFilesMRU.GetY() - popup.GetHeight());

                popupBtnOk.Action += (sender, e) =>
                {
                    int idx = listBox.GetSelected();
                    if (idx >= 0)
                    {
                        Object obj = _fileSystemModel.FileByPath(_filesMRU.EntryAt(idx));
                        if (obj != null)
                        {
                            popup.ClosePopup();
                            FireAcceptCallback(new FileTable.Entry[] {
                                    new FileTable.Entry(_fileSystemModel, obj, _fileSystemModel.Parent(obj) == null)
                                });
                        }
                        else
                        {
                            _filesMRU.RemoveAt(idx);
                        }
                    }
                };

                popupBtnCancel.Action += (sender, e) =>
                {
                    popup.ClosePopup();
                };

                listBox.Callback += (sender, e) =>
                {
                    if (ListBox<String>.CallbackReason_ActionRequested(e.Reason))
                    {
                        int idx = listBox.GetSelected();
                        if (idx >= 0)
                        {
                            Object obj = _fileSystemModel.FileByPath(_filesMRU.EntryAt(idx));
                            if (obj != null)
                            {
                                popup.ClosePopup();
                                FireAcceptCallback(new FileTable.Entry[] {
                                    new FileTable.Entry(_fileSystemModel, obj, _fileSystemModel.Parent(obj) == null)
                                });
                            }
                            else
                            {
                                _filesMRU.RemoveAt(idx);
                            }
                        }
                    }
                };
            }
        }

        private void AddToMRU(FileTable.Entry[] selection)
        {
            foreach (FileTable.Entry entry in selection)
            {
                _filesMRU.Add(entry.GetPath());
            }
            _folderMRU.Add(_fileSystemModel.PathOf(GetCurrentFolder()));
        }

        bool GotoFolderFromMRU(int idx)
        {
            if (idx >= _folderMRU.Entries)
            {
                return false;
            }
            String path = _folderMRU.EntryAt(idx);
            try
            {
                TreeTableNode node = ResolvePath(path);
                SetCurrentNode(node);
                return true;
            }
            catch (ArgumentException ex)
            {
                _folderMRU.RemoveAt(idx);
                return false;
            }
        }

        public class FileFiltersModel : SimpleListModel<String>
        {
            private List<NamedFileFilter> _filters = new List<NamedFileFilter>();

            public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
            public override event EventHandler<ListAllChangedEventArgs> AllChanged;

            public override int Entries
            {
                get
                {
                    return _filters.Count;
                }
            }

            public NamedFileFilter GetFileFilter(int index)
            {
                return _filters[index];
            }

            public int GetNumEntries()
            {
                return _filters.Count;
            }

            public void AddFileFilter(NamedFileFilter filter)
            {
                int index = _filters.Count;
                _filters.Add(filter);
                this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(index, index));
            }

            public void RemoveFileFilter(NamedFileFilter filter)
            {
                int idx = _filters.IndexOf(filter);
                if (idx >= 0)
                {
                    _filters.RemoveAt(idx);
                    this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
                }
            }

            public int FindFilter(NamedFileFilter filter)
            {
                return _filters.IndexOf(filter);
            }

            protected internal void RemoveAll()
            {
                _filters.Clear();
                this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
            }

            public override string EntryAt(int index)
            {
                NamedFileFilter filter = GetFileFilter(index);
                return filter.getDisplayName();
            }
        }

        /**
         * A file object comparator which delegates to a String comprataor to sort based
         * on the name of the file objects.
         */
        public class NameSorter : IComparer<object>
        {
            private FileSystemModel _fileSystemModel;
            private IComparer<String> _nameComparator;

            /**
             * Creates a new comparator which uses {@code NaturalSortComparator.stringComparator} to sort the names
             * @param fsm the file system model
             */
            public NameSorter(FileSystemModel fsm)
            {
                this._fileSystemModel = fsm;
                this._nameComparator = Comparer<string>.Default;
            }

            /**
             * Creates a new comparator which uses the specified String comparator to sort the names
             * @param fsm the file system model
             * @param nameComparator the name comparator
             */
            public NameSorter(FileSystemModel fsm, IComparer<String> nameComparator)
            {
                this._fileSystemModel = fsm;
                this._nameComparator = nameComparator;
            }

            public int Compare(Object o1, Object o2)
            {
                return _nameComparator.Compare(_fileSystemModel.NameOf(o1), _fileSystemModel.NameOf(o2));
            }
        }
    }

    /*public interface Callback {
        public void filesSelected(Object[] files);
        public void canceled();
    }

    public interface Callback2 extends Callback {
        public void folderChanged(Object folder);
        public void selectionChanged(FileTable.Entry[] selection);
    }*/


    public class FileSelectorSelectionChangedEventArgs : EventArgs
    {
        public object[] Selection;
        public FileSelectorSelectionChangedEventArgs(object[] selection)
        {
            this.Selection = selection;
        }
    }

    public class FileSelectorFolderChangedEventArgs : EventArgs
    {
        public Object Folder;
        public FileSelectorFolderChangedEventArgs(Object folder)
        {
            this.Folder = folder;
        }
    }

    public class FileSelectorCancelledEventArgs : EventArgs
    {
    }

    public class FileSelectorFilesSelectedEventArgs : EventArgs
    {
        public object[] Files;
        public FileSelectorFilesSelectedEventArgs(object[] files)
        {
            this.Files = files;
        }
    }
}
