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

        public class NamedFileFilter {
            private String name;
            private FileFilter fileFilter;

            public NamedFileFilter(String name, FileFilter fileFilter) {
                this.name = name;
                this.fileFilter = fileFilter;
            }
            public String getDisplayName() {
                return name;
            }
            public FileFilter getFileFilter() {
                return fileFilter;
            }
        }
    
        public static NamedFileFilter AllFilesFilter = new NamedFileFilter("All files", null);

        private IntegerModel flags;
        private MRUListModel<String> folderMRU;
        MRUListModel<String> filesMRU;

        private TreeComboBox currentFolder;
        private Label labelCurrentFolder;
        private FileTable fileTable;
        private ScrollPane fileTableSP;
        private Button btnUp;
        private Button btnHome;
        private Button btnFolderMRU;
        private Button btnFilesMRU;
        private Button btnOk;
        private Button btnCancel;
        private Button btnRefresh;
        private Button btnShowFolders;
        private Button btnShowHidden;
        private ComboBox<String> fileFilterBox;
        private FileFiltersModel fileFiltersModel;
        private EditFieldAutoCompletionWindow autoCompletion;

        private bool allowFolderSelection;
        private NamedFileFilter activeFileFilter;

        FileSystemModel fsm;
        private FileSystemTreeModel model;

        private Widget userWidgetBottom;
        private Widget userWidgetRight;

        private Object fileToSelectOnSetCurrentNode;

        /**
         * Create a FileSelector without persistent state
         */
        public FileSelector() : this(null, null)
        {
        }

        class FileSelectorPathResolver : TreeComboBox.PathResolver
        {
            private FileSelector fileSelector;

            public FileSelectorPathResolver(FileSelector fileSelector)
            {
                this.fileSelector = fileSelector;
            }

            public TreeTableNode resolvePath(TreeTableModel model, string path)
            {
                return this.fileSelector.resolvePath(path);
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
                flags = new PersistentIntegerModel(prefs, prefsKey + "_Flags", 0, 0xFFFF, 0);
                folderMRU = new PersistentMRUListModel<String>(10, typeof(String), prefs, prefsKey + "_foldersMRU");
                filesMRU = new PersistentMRUListModel<String>(20, typeof(String), prefs, prefsKey + "_filesMRU");
            }
            else
            {
                flags = new SimpleIntegerModel(0, 0xFFFF, 0);
                folderMRU = new SimpleMRUListModel<String>(10);
                filesMRU = new SimpleMRUListModel<String>(20);
            }

            currentFolder = new TreeComboBox();
            currentFolder.setTheme("currentFolder");
            fileTable = new FileTable();
            fileTable.setTheme("fileTable");
            fileTable.SelectionChanged += (sender, e) =>
            {
                this.selectionChanged();
            };

            btnUp = new Button();
            btnUp.setTheme("buttonUp");
            btnUp.Action += (sender, e) =>
            {
                goOneLevelUp();
            };

            btnHome = new Button();
            btnHome.setTheme("buttonHome");
            btnHome.Action += (sender, e) =>
            {
                goHome();
            };

            btnFolderMRU = new Button();
            btnFolderMRU.setTheme("buttonFoldersMRU");
            btnFolderMRU.Action += (sender, e) =>
            {
                showFolderMRU();
            };

            btnFilesMRU = new Button();
            btnFilesMRU.setTheme("buttonFilesMRU");

            btnFilesMRU.Action += (sender, e) =>
            {
                showFilesMRU();
            };

            btnOk = new Button();
            btnOk.setTheme("buttonOk");
            btnOk.Action += (sender, e) =>
            {
                acceptSelection();
            };

            btnCancel = new Button();
            btnCancel.setTheme("buttonCancel");
            btnCancel.Action += (sender, e) =>
            {
                fireCanceled();
            };

            currentFolder.setPathResolver(new FileSelectorPathResolver(this));
            currentFolder.SelectedNodeChanged += (sender, e) =>
            {
                setCurrentNode(e.Node, e.PreviousChildNode);
            };

            autoCompletion = new EditFieldAutoCompletionWindow(currentFolder.getEditField());
            //autoCompletion.setUseInvokeAsync(true);
            currentFolder.getEditField().setAutoCompletionWindow(autoCompletion);

            fileTable.setAllowMultiSelection(true);
            fileTable.DoubleClick += (sender, e) =>
            {
                this.acceptSelection();
            };

            activeFileFilter = AllFilesFilter;
            fileFiltersModel = new FileFiltersModel();
            fileFilterBox = new ComboBox<String>(fileFiltersModel);
            fileFilterBox.setTheme("fileFiltersBox");
            fileFilterBox.setComputeWidthFromModel(true);
            fileFilterBox.setVisible(false);
            fileFilterBox.SelectionChanged += (sender, e) =>
            {
                fileFilterChanged();
            };

            labelCurrentFolder = new Label("Folder");
            labelCurrentFolder.setLabelFor(currentFolder);

            fileTableSP = new ScrollPane(fileTable);


            btnRefresh = new Button();
            btnRefresh.setTheme("buttonRefresh");
            btnRefresh.Action += BtnRefresh_Action;

            btnShowFolders = new Button(new ToggleButtonModel(new BitFieldBooleanModel(flags, 0), true));
            btnShowFolders.setTheme("buttonShowFolders");
            btnShowFolders.Action += BtnRefresh_Action;

            btnShowHidden = new Button(new ToggleButtonModel(new BitFieldBooleanModel(flags, 1), false));
            btnShowHidden.setTheme("buttonShowHidden");
            btnShowHidden.Action += BtnRefresh_Action;

            addActionMapping("goOneLevelUp", "goOneLevelUp");
            addActionMapping("acceptSelection", "acceptSelection");
        }

        private void BtnRefresh_Action(object sender, ButtonActionEventArgs e)
        {
            this.refreshFileTable();
        }

        protected void createLayout()
        {
            setHorizontalGroup(null);
            setVerticalGroup(null);
            removeAllChildren();

            add(fileTableSP);
            add(fileFilterBox);
            add(btnOk);
            add(btnCancel);
            add(btnRefresh);
            add(btnShowFolders);
            add(btnShowHidden);
            add(labelCurrentFolder);
            add(currentFolder);
            add(btnFolderMRU);
            add(btnUp);

            Group hCurrentFolder = createSequentialGroup()
                    .addWidget(labelCurrentFolder)
                    .addWidget(currentFolder)
                    .addWidget(btnFolderMRU)
                    .addWidget(btnUp)
                    .addWidget(btnHome);
            Group vCurrentFolder = createParallelGroup()
                    .addWidget(labelCurrentFolder)
                    .addWidget(currentFolder)
                    .addWidget(btnFolderMRU)
                    .addWidget(btnUp)
                    .addWidget(btnHome);

            Group hButtonGroup = createSequentialGroup()
                    .addWidget(btnRefresh)
                    .addGap(MEDIUM_GAP)
                    .addWidget(btnShowFolders)
                    .addWidget(btnShowHidden)
                    .addWidget(fileFilterBox)
                    .addGap("buttonBarLeft")
                    .addWidget(btnFilesMRU)
                    .addGap("buttonBarSpacer")
                    .addWidget(btnOk)
                    .addGap("buttonBarSpacer")
                    .addWidget(btnCancel)
                    .addGap("buttonBarRight");
            Group vButtonGroup = createParallelGroup()
                    .addWidget(btnRefresh)
                    .addWidget(btnShowFolders)
                    .addWidget(btnShowHidden)
                    .addWidget(fileFilterBox)
                    .addWidget(btnFilesMRU)
                    .addWidget(btnOk)
                    .addWidget(btnCancel);

            Group horz = createParallelGroup()
                    .addGroup(hCurrentFolder)
                    .addWidget(fileTableSP);

            Group vert = createSequentialGroup()
                    .addGroup(vCurrentFolder)
                    .addWidget(fileTableSP);

            if (userWidgetBottom != null)
            {
                horz.addWidget(userWidgetBottom);
                vert.addWidget(userWidgetBottom);
            }

            if (userWidgetRight != null)
            {
                horz = createParallelGroup().addGroup(createSequentialGroup()
                        .addGroup(horz)
                        .addWidget(userWidgetRight));
                vert = createSequentialGroup().addGroup(createParallelGroup()
                        .addGroup(vert)
                        .addWidget(userWidgetRight));
            }

            setHorizontalGroup(horz.addGroup(hButtonGroup));
            setVerticalGroup(vert.addGroup(vButtonGroup));
        }

        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            createLayout();
        }

        public FileSystemModel getFileSystemModel()
        {
            return fsm;
        }

        public void setFileSystemModel(FileSystemModel fsm)
        {
            this.fsm = fsm;
            if (fsm == null)
            {
                model = null;
                currentFolder.setModel(null);
                fileTable.setCurrentFolder(null, null);
                autoCompletion.setDataSource(null);
            }
            else
            {
                model = new FileSystemTreeModel(fsm);
                model.SetSorter(new NameSorter(fsm));
                currentFolder.setModel(model);
                currentFolder.setSeparator(fsm.Separator);
                autoCompletion.setDataSource(new FileSystemAutoCompletionDataSource(fsm,
                        FolderFilter.Instance));
                if (!gotoFolderFromMRU(0) && !goHome())
                {
                    setCurrentNode(model);
                }
            }
        }

        public bool getAllowMultiSelection()
        {
            return fileTable.getAllowMultiSelection();
        }

        /**
         * Controls if multi selection is allowed.
         *
         * Default is true.
         *
         * @param allowMultiSelection true if multiple files can be selected.
         */
        public void setAllowMultiSelection(bool allowMultiSelection)
        {
            fileTable.setAllowMultiSelection(allowMultiSelection);
        }

        public bool getAllowFolderSelection()
        {
            return allowFolderSelection;
        }

        /**
         * Controls if folders can be selected. If false then the "Ok" button
         * is disabled when a folder is selected.
         *
         * Default is false.
         *
         * @param allowFolderSelection true if folders can be selected
         */
        public void setAllowFolderSelection(bool allowFolderSelection)
        {
            this.allowFolderSelection = allowFolderSelection;
            selectionChanged();
        }

        public bool getAllowHorizontalScrolling()
        {
            return fileTableSP.getFixed() != ScrollPane.Fixed.HORIZONTAL;
        }

        /**
         * Controls if the file table allows horizontal scrolling or not.
         * 
         * Default is true.
         * 
         * @param allowHorizontalScrolling true if horizontal scrolling is allowed
         */
        public void setAllowHorizontalScrolling(bool allowHorizontalScrolling)
        {
            fileTableSP.setFixed(allowHorizontalScrolling
                    ? ScrollPane.Fixed.NONE
                    : ScrollPane.Fixed.HORIZONTAL);
        }

        public Widget getUserWidgetBottom()
        {
            return userWidgetBottom;
        }

        public void setUserWidgetBottom(Widget userWidgetBottom)
        {
            this.userWidgetBottom = userWidgetBottom;
            createLayout();
        }

        public Widget getUserWidgetRight()
        {
            return userWidgetRight;
        }

        public void setUserWidgetRight(Widget userWidgetRight)
        {
            this.userWidgetRight = userWidgetRight;
            createLayout();
        }

        public FileTable getFileTable()
        {
            return fileTable;
        }

        public void setOkButtonEnabled(bool enabled)
        {
            btnOk.setEnabled(enabled);
        }

        public Object getCurrentFolder()
        {
            Object node = currentFolder.getCurrentNode();
            if (node is FolderNode)
            {
                return ((FolderNode)node).Folder;
            }
            else
            {
                return null;
            }
        }

        public bool setCurrentFolder(Object folder)
        {
            FolderNode node = model.NodeForFolder(folder);
            if (node != null)
            {
                setCurrentNode(node);
                return true;
            }
            return false;
        }

        public bool selectFile(Object file)
        {
            if (fsm == null)
            {
                return false;
            }
            Object parent = fsm.Parent(file);
            if (setCurrentFolder(parent))
            {
                return fileTable.setSelection(file);
            }
            return false;
        }

        public void clearSelection()
        {
            fileTable.clearSelection();
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
        public void addFileFilter(NamedFileFilter filter)
        {
            if (filter == null)
            {
                throw new NullReferenceException("filter");
            }
            fileFiltersModel.addFileFilter(filter);
            fileFilterBox.setVisible(fileFiltersModel.getNumEntries() > 0);
            if (fileFilterBox.getSelected() < 0)
            {
                fileFilterBox.setSelected(0);
            }
        }

        public void removeFileFilter(NamedFileFilter filter)
        {
            if (filter == null)
            {
                throw new NullReferenceException("filter");
            }
            fileFiltersModel.removeFileFilter(filter);
            if (fileFiltersModel.getNumEntries() == 0)
            {
                fileFilterBox.setVisible(false);
                setFileFilter(AllFilesFilter);
            }
        }

        public void removeAllFileFilters()
        {
            fileFiltersModel.removeAll();
            fileFilterBox.setVisible(false);
            setFileFilter(AllFilesFilter);
        }

        public void setFileFilter(NamedFileFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }
            int idx = fileFiltersModel.findFilter(filter);
            if (idx < 0)
            {
                throw new ArgumentOutOfRangeException("filter not registered");
            }
            fileFilterBox.setSelected(idx);
        }

        public NamedFileFilter getFileFilter()
        {
            return activeFileFilter;
        }

        public bool getShowFolders()
        {
            return btnShowFolders.getModel().Selected;
        }

        public void setShowFolders(bool showFolders)
        {
            btnShowFolders.getModel().Selected = showFolders;
        }

        public bool getShowHidden()
        {
            return btnShowHidden.getModel().Selected;
        }

        public void setShowHidden(bool showHidden)
        {
            btnShowHidden.getModel().Selected = showHidden;
        }

        public void goOneLevelUp()
        {
            TreeTableNode node = currentFolder.getCurrentNode();
            TreeTableNode parent = node.Parent;
            if (parent != null)
            {
                setCurrentNode(parent, node);
            }
        }

        public bool goHome()
        {
            if (fsm != null)
            {
                Object folder = fsm.SpecialFolder(DotNetFileSystemModel.USERPROFILE_FOLDER);
                if (folder != null)
                {
                    return setCurrentFolder(folder);
                }
            }
            return false;
        }

        public void acceptSelection()
        {
            FileTable.Entry[] selection = fileTable.getSelection();
            if (selection.Length == 1)
            {
                FileTable.Entry entry = selection[0];
                if (entry != null && entry.isFolder)
                {
                    setCurrentFolder(entry.obj);
                    return;
                }
            }
            fireAcceptCallback(selection);
        }

        void fileFilterChanged()
        {
            int idx = fileFilterBox.getSelected();
            if (idx >= 0)
            {
                NamedFileFilter filter = fileFiltersModel.getFileFilter(idx);
                activeFileFilter = filter;
                fileTable.setFileFilter(filter.getFileFilter());
            }
        }

        void fireAcceptCallback(FileTable.Entry[] selection)
        {
            if (this.FilesSelected != null)
            {
                Object[] objects = new Object[selection.Length];
                for (int i = 0; i < selection.Length; i++)
                {
                    FileTable.Entry e = selection[i];
                    if (e.isFolder && !allowFolderSelection)
                    {
                        return;
                    }
                    objects[i] = e.obj;
                }
                addToMRU(selection);
                this.FilesSelected.Invoke(this, new FileSelectorFilesSelectedEventArgs(objects));
            }
        }

        void fireCanceled()
        {
            if (this.Cancelled != null)
            {
                this.Cancelled.Invoke(this, new FileSelectorCancelledEventArgs());
            }
        }

        void selectionChanged()
        {
            bool foldersSelected = false;
            bool filesSelected = false;
            FileTable.Entry[] selection = fileTable.getSelection();
            foreach (FileTable.Entry entry in selection)
            {
                if (entry.isFolder)
                {
                    foldersSelected = true;
                }
                else
                {
                    filesSelected = true;
                }
            }
            if (allowFolderSelection)
            {
                btnOk.setEnabled(filesSelected || foldersSelected);
            }
            else
            {
                btnOk.setEnabled(filesSelected && !foldersSelected);
            }

            if (this.SelectionChanged != null)
            {
                this.SelectionChanged.Invoke(this, new FileSelectorSelectionChangedEventArgs(selection));
            }
        }

        protected void setCurrentNode(TreeTableNode node, TreeTableNode childToSelect)
        {
            if (childToSelect is FolderNode)
            {
                fileToSelectOnSetCurrentNode = ((FolderNode)childToSelect).Folder;
            }
            setCurrentNode(node);
        }

        protected void setCurrentNode(TreeTableNode node)
        {
            currentFolder.setCurrentNode(node);
            refreshFileTable();
            if (this.FolderChanged != null)
            {
                Object curFolder = getCurrentFolder();
                this.FolderChanged.Invoke(this, new FileSelectorFolderChangedEventArgs(curFolder));
            }
            if (fileToSelectOnSetCurrentNode != null)
            {
                fileTable.setSelection(fileToSelectOnSetCurrentNode);
                fileToSelectOnSetCurrentNode = null;
            }
        }

        void refreshFileTable()
        {
            fileTable.setShowFolders(btnShowFolders.getModel().Selected);
            fileTable.setShowHidden(btnShowHidden.getModel().Selected);
            fileTable.setCurrentFolder(fsm, getCurrentFolder());
        }

        TreeTableNode resolvePath(String path)
        {
            Object obj = fsm.FileByPath(path);
            fileToSelectOnSetCurrentNode = null;
            if (obj != null)
            {
                if (fsm.IsFile(obj))
                {
                    fileToSelectOnSetCurrentNode = obj;
                    obj = fsm.Parent(obj);
                }
                FolderNode node = model.NodeForFolder(obj);
                if (node != null)
                {
                    return node;
                }
            }
            throw new ArgumentException("Could not resolve: " + path);
        }

        void showFolderMRU()
        {
            PopupWindow popup = new PopupWindow(this);
            ListBox<String> listBox = new ListBox<String>(folderMRU);
            popup.setTheme("fileselector-folderMRUpopup");
            popup.add(listBox);
            if (popup.openPopup())
            {
                popup.setInnerSize(getInnerWidth() * 2 / 3, getInnerHeight() * 2 / 3);
                popup.setPosition(btnFolderMRU.getX() - popup.getWidth(), btnFolderMRU.getY());
                listBox.Callback += (sender, e) =>
                {
                    if (ListBox<String>.CallbackReason_ActionRequested(e.Reason))
                    {
                        popup.closePopup();
                        int idx = listBox.getSelected();
                        if (idx >= 0)
                        {
                            gotoFolderFromMRU(idx);
                        }
                    }
                };
            }
        }

        void showFilesMRU()
        {
            PopupWindow popup = new PopupWindow(this);
            DialogLayout layout = new DialogLayout();
            ListBox<String> listBox = new ListBox<String>(filesMRU);
            Button popupBtnOk = new Button();
            Button popupBtnCancel = new Button();
            popupBtnOk.setTheme("buttonOk");
            popupBtnCancel.setTheme("buttonCancel");
            popup.setTheme("fileselector-filesMRUpopup");
            popup.add(layout);
            layout.add(listBox);
            layout.add(popupBtnOk);
            layout.add(popupBtnCancel);

            DialogLayout.Group hBtnGroup = layout.createSequentialGroup()
                    .addGap().addWidget(popupBtnOk).addWidget(popupBtnCancel);
            DialogLayout.Group vBtnGroup = layout.createParallelGroup()
                    .addWidget(popupBtnOk).addWidget(popupBtnCancel);
            layout.setHorizontalGroup(layout.createParallelGroup().addWidget(listBox).addGroup(hBtnGroup));
            layout.setVerticalGroup(layout.createSequentialGroup().addWidget(listBox).addGroup(vBtnGroup));

            if (popup.openPopup())
            {
                popup.setInnerSize(getInnerWidth() * 2 / 3, getInnerHeight() * 2 / 3);
                popup.setPosition(getInnerX() + (getInnerWidth() - popup.getWidth()) / 2, btnFilesMRU.getY() - popup.getHeight());

                popupBtnOk.Action += (sender, e) =>
                {
                    int idx = listBox.getSelected();
                    if (idx >= 0)
                    {
                        Object obj = fsm.FileByPath(filesMRU.EntryAt(idx));
                        if (obj != null)
                        {
                            popup.closePopup();
                            fireAcceptCallback(new FileTable.Entry[] {
                                    new FileTable.Entry(fsm, obj, fsm.Parent(obj) == null)
                                });
                        }
                        else
                        {
                            filesMRU.RemoveAt(idx);
                        }
                    }
                };

                popupBtnCancel.Action += (sender, e) =>
                {
                    popup.closePopup();
                };

                listBox.Callback += (sender, e) =>
                {
                    if (ListBox<String>.CallbackReason_ActionRequested(e.Reason))
                    {
                        int idx = listBox.getSelected();
                        if (idx >= 0)
                        {
                            Object obj = fsm.FileByPath(filesMRU.EntryAt(idx));
                            if (obj != null)
                            {
                                popup.closePopup();
                                fireAcceptCallback(new FileTable.Entry[] {
                                    new FileTable.Entry(fsm, obj, fsm.Parent(obj) == null)
                                });
                            }
                            else
                            {
                                filesMRU.RemoveAt(idx);
                            }
                        }
                    }
                };
            }
        }

        private void addToMRU(FileTable.Entry[] selection)
        {
            foreach (FileTable.Entry entry in selection)
            {
                filesMRU.Add(entry.getPath());
            }
            folderMRU.Add(fsm.PathOf(getCurrentFolder()));
        }

        bool gotoFolderFromMRU(int idx)
        {
            if (idx >= folderMRU.Entries)
            {
                return false;
            }
            String path = folderMRU.EntryAt(idx);
            try
            {
                TreeTableNode node = resolvePath(path);
                setCurrentNode(node);
                return true;
            }
            catch (ArgumentException ex)
            {
                folderMRU.RemoveAt(idx);
                return false;
            }
        }

        public class FileFiltersModel : SimpleListModel<String>
        {
            private List<NamedFileFilter> filters = new List<NamedFileFilter>();

            public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
            public override event EventHandler<ListAllChangedEventArgs> AllChanged;

            public override int Entries
            {
                get
                {
                    return filters.Count;
                }
            }

            public NamedFileFilter getFileFilter(int index)
            {
                return filters[index];
            }

            public int getNumEntries()
            {
                return filters.Count;
            }

            public void addFileFilter(NamedFileFilter filter)
            {
                int index = filters.Count;
                filters.Add(filter);
                this.EntriesInserted.Invoke(this, new ListSubsetChangedEventArgs(index, index));
            }

            public void removeFileFilter(NamedFileFilter filter)
            {
                int idx = filters.IndexOf(filter);
                if (idx >= 0)
                {
                    filters.RemoveAt(idx);
                    this.EntriesDeleted.Invoke(this, new ListSubsetChangedEventArgs(idx, idx));
                }
            }

            public int findFilter(NamedFileFilter filter)
            {
                return filters.IndexOf(filter);
            }

            protected internal void removeAll()
            {
                filters.Clear();
                this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
            }

            public override string EntryAt(int index)
            {
                NamedFileFilter filter = getFileFilter(index);
                return filter.getDisplayName();
            }
        }

        /**
         * A file object comparator which delegates to a String comprataor to sort based
         * on the name of the file objects.
         */
        public class NameSorter : IComparer<object>
        {
            private FileSystemModel fsm;
            private IComparer<String> nameComparator;

            /**
             * Creates a new comparator which uses {@code NaturalSortComparator.stringComparator} to sort the names
             * @param fsm the file system model
             */
            public NameSorter(FileSystemModel fsm)
            {
                this.fsm = fsm;
                this.nameComparator = Comparer<string>.Default;
            }

            /**
             * Creates a new comparator which uses the specified String comparator to sort the names
             * @param fsm the file system model
             * @param nameComparator the name comparator
             */
            public NameSorter(FileSystemModel fsm, IComparer<String> nameComparator)
            {
                this.fsm = fsm;
                this.nameComparator = nameComparator;
            }

            public int Compare(Object o1, Object o2)
            {
                return nameComparator.Compare(fsm.NameOf(o1), fsm.NameOf(o2));
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
