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
using static XNATWL.Utils.Logger;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class FolderBrowser : Widget
    {
        FileSystemModel _fileSystemModel;
        ListBox<Object> _listBox;
        FolderModel _model;
        private BoxLayout _curFolderGroup;

        IComparer<String> _folderComparator;
        private Object _currentFolder;

        public event EventHandler<FolderBrowserSelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<FolderBrowserCompletedEventArgs> Completed;

        public FolderBrowser() : this(DotNetFileSystemModel.getInstance())
        {

        }

        public FolderBrowser(FileSystemModel fsm)
        {
            if (fsm == null)
            {
                throw new NullReferenceException("fsm");
            }

            this._fileSystemModel = fsm;
            this._model = new FolderModel(this);
            this._listBox = new ListBox<Object>(_model);
            this._curFolderGroup = new BoxLayout();

            _curFolderGroup.SetTheme("currentpathbox");
            _curFolderGroup.SetScroll(true);
            _curFolderGroup.SetClip(true);
            _curFolderGroup.SetAlignment(Alignment.BOTTOM);

            _listBox.Callback += Listbox_Callback;
            /*listbox.addCallback(new CallbackWithReason<ListBox.CallbackReason>() {
                private Object lastSelection;
                public void callback(ListBox.CallbackReason reason) {
                    if(listbox.getSelected() != ListBox.NO_SELECTION) {
                        if(reason.actionRequested()) {
                            setCurrentFolder(model.getFolder(listbox.getSelected()));
                        }
                    }
                    Object selection = getSelectedFolder();
                    if(selection != lastSelection) {
                        lastSelection = selection;
                        fireSelectionChangedCallback();
                    }
                }
            });*/

            Add(_listBox);
            Add(_curFolderGroup);

            SetCurrentFolder(null);
        }

        private Object lastSelection;
        private void Listbox_Callback(object sender, ListBoxEventArgs e)
        {
            if (_listBox.GetSelected() != ListBox<Object>.NO_SELECTION)
            {
                if (ListBox<Object>.CallbackReason_ActionRequested(e.Reason))
                {
                    SetCurrentFolder(_model.GetFolder(_listBox.GetSelected()));
                }
            }

            Object selection = GetSelectedFolder();
            if (selection != lastSelection)
            {
                lastSelection = selection;
                this.SelectionChanged.Invoke(this, new FolderBrowserSelectionChangedEventArgs());
            }
        }

        public IComparer<String> GetFolderComparator()
        {
            return _folderComparator;
        }

        public void SetFolderComparator(IComparer<String> folderComparator)
        {
            this._folderComparator = folderComparator;
        }

        public FileSystemModel GetFileSystemModel()
        {
            return _fileSystemModel;
        }

        /**
         * Get the current displayed folder
         * @return the displayed folder or null if root is displayed
         */
        public Object GetCurrentFolder()
        {
            return _currentFolder;
        }

        public bool SetCurrentFolder(Object folder)
        {
            if (_model.ListFolders(folder))
            {
                // if we show root and it has only a single entry go directly into it
                if (folder == null && _model.Entries == 1)
                {
                    if (SetCurrentFolder(_model.GetFolder(0)))
                    {
                        return true;
                    }
                }

                _currentFolder = folder;
                _listBox.SetSelected(ListBox<Object>.NO_SELECTION);

                RebuildCurrentFolderGroup();

                this.Completed.Invoke(this, new FolderBrowserCompletedEventArgs());
                return true;
            }
            return false;
        }

        public bool GoToParentFolder()
        {
            if (_currentFolder != null)
            {
                Object current = _currentFolder;
                if (SetCurrentFolder(_fileSystemModel.Parent(current)))
                {
                    SelectFolder(current);
                    return true;
                }
            }
            return false;
        }

        /**
         * Get the current selected folder in the list box
         * @return a folder or null if nothing is selected
         */
        public Object GetSelectedFolder()
        {
            if (_listBox.GetSelected() != ListBox<Object>.NO_SELECTION)
            {
                return _model.GetFolder(_listBox.GetSelected());
            }
            return null;
        }

        public bool SelectFolder(Object current)
        {
            int idx = _model.FindFolder(current);
            _listBox.SetSelected(idx);
            return idx != ListBox<Object>.NO_SELECTION;
        }

        //@Override
        public override bool HandleEvent(Event evt)
        {
            if (evt.IsKeyPressedEvent())
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_BACK:
                        GoToParentFolder();
                        return true;
                }
            }
            return base.HandleEvent(evt);
        }

        private void RebuildCurrentFolderGroup()
        {
            _curFolderGroup.RemoveAllChildren();
            RecursiveAddFolder(_currentFolder, null);
        }

        private void RecursiveAddFolder(Object folder, Object subFolder)
        {
            if (folder != null)
            {
                RecursiveAddFolder(_fileSystemModel.Parent(folder), folder);
            }
            if (_curFolderGroup.GetNumChildren() > 0)
            {
                Label l = new Label(_fileSystemModel.Separator);
                l.SetTheme("pathseparator");
                _curFolderGroup.Add(l);
            }
            String name = GetFolderName(folder);
            if (name.EndsWith(_fileSystemModel.Separator))
            {
                name = name.Substring(0, name.Length - 1);
            }
            Button btn = new Button(name);
            btn.Action += (sender, e) =>
            {
                if (SetCurrentFolder(folder))
                {
                    SelectFolder(subFolder);
                }
                _listBox.RequestKeyboardFocus();
            };
            /*btn.addCallback(new Runnable() {
                public void run() {
                    if(setCurrentFolder(folder)) {
                        selectFolder(subFolder);
                    }
                    listbox.requestKeyboardFocus();
                }
            });*/
            btn.SetTheme("pathbutton");
            _curFolderGroup.Add(btn);
        }

        //@Override
        public override void AdjustSize()
        {
        }

        //@Override
        protected override void Layout()
        {
            _curFolderGroup.SetPosition(GetInnerX(), GetInnerY());
            _curFolderGroup.SetSize(GetInnerWidth(), _curFolderGroup.GetHeight());
            _listBox.SetPosition(GetInnerX(), _curFolderGroup.GetBottom());
            _listBox.SetSize(GetInnerWidth(), Math.Max(0, GetInnerBottom() - _listBox.GetY()));
        }

        String GetFolderName(Object folder)
        {
            if (folder != null)
            {
                return _fileSystemModel.NameOf(folder);
            }
            else
            {
                return "ROOT";
            }
        }

        class FolderModel : SimpleListModel<Object>
        {
            private Object[] _folders = new Object[0];
            private FolderBrowser _folderBrowser;

            public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
            public override event EventHandler<ListAllChangedEventArgs> AllChanged;

            public FolderModel(FolderBrowser folderBrowser)
            {
                this._folderBrowser = folderBrowser;
            }

            public void FireAllChanged()
            {
                this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
            }

            public bool ListFolders(Object parent)
            {
                Object[] newFolders;
                if (parent == null)
                {
                    newFolders = this._folderBrowser._fileSystemModel.ListRoots();
                }
                else
                {
                    newFolders = this._folderBrowser._fileSystemModel.ListFolder(parent, FolderFilter.Instance);
                }
                if (newFolders == null)
                {
                    Logger.GetLogger(typeof(FolderModel)).Log(Level.WARNING, "can''t list folder: " + parent.ToString());
                    return false;
                }
                Array.Sort(newFolders, new FileSelector.NameSorter(this._folderBrowser._fileSystemModel, (this._folderBrowser._folderComparator != null)
                        ? this._folderBrowser._folderComparator
                        : Comparer<string>.Default));
                _folders = newFolders;

                this.FireAllChanged();
                return true;
            }

            public override int Entries
            {
                get
                {
                    return _folders.Length;
                }
            }

            public Object GetFolder(int index)
            {
                return _folders[index];
            }

            public override object EntryAt(int index)
            {
                Object folder = GetFolder(index);
                return this._folderBrowser.GetFolderName(folder);
            }

            public int FindFolder(Object folder)
            {
                int idx = this._folderBrowser._fileSystemModel.Find(_folders, folder);
                return (idx < 0) ? ListBox<Object>.NO_SELECTION : idx;
            }
        }
    }

    public class FolderBrowserCompletedEventArgs : EventArgs
    {
    }

    public class FolderBrowserSelectionChangedEventArgs : EventArgs
    {
    }
}
