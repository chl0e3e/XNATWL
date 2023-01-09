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

        FileSystemModel fsm;
        ListBox<Object> listbox;
        FolderModel model;
        private BoxLayout curFolderGroup;
        private Runnable[] selectionChangedCallbacks;

        IComparer<String> folderComparator;
        private Object currentFolder;
        private Runnable[] callbacks;
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

            this.fsm = fsm;
            this.model = new FolderModel(this);
            this.listbox = new ListBox<Object>(model);
            this.curFolderGroup = new BoxLayout();

            curFolderGroup.setTheme("currentpathbox");
            curFolderGroup.setScroll(true);
            curFolderGroup.setClip(true);
            curFolderGroup.setAlignment(Alignment.BOTTOM);

            listbox.Callback += Listbox_Callback;
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

            add(listbox);
            add(curFolderGroup);

            setCurrentFolder(null);
        }

        private Object lastSelection;
        private void Listbox_Callback(object sender, ListBoxEventArgs e)
        {
            if (listbox.getSelected() != ListBox<Object>.NO_SELECTION)
            {
                if (ListBox<Object>.CallbackReason_ActionRequested(e.Reason))
                {
                    setCurrentFolder(model.getFolder(listbox.getSelected()));
                }
            }

            Object selection = getSelectedFolder();
            if (selection != lastSelection)
            {
                lastSelection = selection;
                this.SelectionChanged.Invoke(this, new FolderBrowserSelectionChangedEventArgs());
            }
        }

        public IComparer<String> getFolderComparator()
        {
            return folderComparator;
        }

        public void setFolderComparator(IComparer<String> folderComparator)
        {
            this.folderComparator = folderComparator;
        }

        public FileSystemModel getFileSystemModel()
        {
            return fsm;
        }

        /**
         * Get the current displayed folder
         * @return the displayed folder or null if root is displayed
         */
        public Object getCurrentFolder()
        {
            return currentFolder;
        }

        public bool setCurrentFolder(Object folder)
        {
            if (model.listFolders(folder))
            {
                // if we show root and it has only a single entry go directly into it
                if (folder == null && model.Entries == 1)
                {
                    if (setCurrentFolder(model.getFolder(0)))
                    {
                        return true;
                    }
                }

                currentFolder = folder;
                listbox.setSelected(ListBox<Object>.NO_SELECTION);

                rebuildCurrentFolderGroup();

                this.Completed.Invoke(this, new FolderBrowserCompletedEventArgs());
                return true;
            }
            return false;
        }

        public bool goToParentFolder()
        {
            if (currentFolder != null)
            {
                Object current = currentFolder;
                if (setCurrentFolder(fsm.Parent(current)))
                {
                    selectFolder(current);
                    return true;
                }
            }
            return false;
        }

        /**
         * Get the current selected folder in the list box
         * @return a folder or null if nothing is selected
         */
        public Object getSelectedFolder()
        {
            if (listbox.getSelected() != ListBox<Object>.NO_SELECTION)
            {
                return model.getFolder(listbox.getSelected());
            }
            return null;
        }

        public bool selectFolder(Object current)
        {
            int idx = model.findFolder(current);
            listbox.setSelected(idx);
            return idx != ListBox<Object>.NO_SELECTION;
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (evt.isKeyPressedEvent())
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_BACK:
                        goToParentFolder();
                        return true;
                }
            }
            return base.handleEvent(evt);
        }

        private void rebuildCurrentFolderGroup()
        {
            curFolderGroup.removeAllChildren();
            recursiveAddFolder(currentFolder, null);
        }

        private void recursiveAddFolder(Object folder, Object subFolder)
        {
            if (folder != null)
            {
                recursiveAddFolder(fsm.Parent(folder), folder);
            }
            if (curFolderGroup.getNumChildren() > 0)
            {
                Label l = new Label(fsm.Separator);
                l.setTheme("pathseparator");
                curFolderGroup.add(l);
            }
            String name = getFolderName(folder);
            if (name.EndsWith(fsm.Separator))
            {
                name = name.Substring(0, name.Length - 1);
            }
            Button btn = new Button(name);
            btn.Action += (sender, e ) =>
            {
                if (setCurrentFolder(folder))
                {
                    selectFolder(subFolder);
                }
                listbox.requestKeyboardFocus();
            };
            /*btn.addCallback(new Runnable() {
                public void run() {
                    if(setCurrentFolder(folder)) {
                        selectFolder(subFolder);
                    }
                    listbox.requestKeyboardFocus();
                }
            });*/
            btn.setTheme("pathbutton");
            curFolderGroup.add(btn);
        }

        //@Override
        public override void adjustSize()
        {
        }

        //@Override
        protected override void layout()
        {
            curFolderGroup.setPosition(getInnerX(), getInnerY());
            curFolderGroup.setSize(getInnerWidth(), curFolderGroup.getHeight());
            listbox.setPosition(getInnerX(), curFolderGroup.getBottom());
            listbox.setSize(getInnerWidth(), Math.Max(0, getInnerBottom() - listbox.getY()));
        }

        String getFolderName(Object folder)
        {
            if (folder != null)
            {
                return fsm.NameOf(folder);
            }
            else
            {
                return "ROOT";
            }
        }

        class FolderModel : SimpleListModel<Object>
        {
            private Object[] folders = new Object[0];
            private FolderBrowser folderBrowser;

            public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
            public override event EventHandler<ListAllChangedEventArgs> AllChanged;

            public FolderModel(FolderBrowser folderBrowser)
            {
                this.folderBrowser = folderBrowser;
            }

            public void FireAllChanged()
            {
                this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
            }

            public bool listFolders(Object parent)
            {
                Object[] newFolders;
                if (parent == null)
                {
                    newFolders = this.folderBrowser.fsm.ListRoots();
                }
                else
                {
                    newFolders = this.folderBrowser.fsm.ListFolder(parent, FolderFilter.Instance);
                }
                if (newFolders == null)
                {
                    Logger.GetLogger(typeof(FolderModel)).log(Level.WARNING, "can''t list folder: " + parent.ToString());
                    return false;
                }
                Array.Sort(newFolders, new FileSelector.NameSorter(this.folderBrowser.fsm, (this.folderBrowser.folderComparator != null)
                        ? this.folderBrowser.folderComparator
                        : Comparer<string>.Default));
                folders = newFolders;

                this.FireAllChanged();
                return true;
            }

            public override int Entries
            {
                get
                {
                    return folders.Length;
                }
            }

            public Object getFolder(int index)
            {
                return folders[index];
            }

            public override object EntryAt(int index)
            {
                Object folder = getFolder(index);
                return this.folderBrowser.getFolderName(folder);
            }

            public int findFolder(Object folder)
            {
                int idx = this.folderBrowser.fsm.Find(folders, folder);
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
