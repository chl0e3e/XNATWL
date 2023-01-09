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
using System.Text;
using XNATWL.IO;

namespace XNATWL.Model
{
    public class FileSystemTreeModel : AbstractTreeTableModel
    {
        private FileSystemModel fsm;
        private bool includeLastModified;

        internal IComparer<object> sorter;

        public override int Columns
        {
            get
            {
                return includeLastModified ? 2 : 1;
            }
        }

        public override event EventHandler<ColumnsChangedEventArgs> ColumnInserted;
        public override event EventHandler<ColumnsChangedEventArgs> ColumnDeleted;
        public override event EventHandler<ColumnHeaderChangedEventArgs> ColumnHeaderChanged;

        public FileSystemTreeModel(FileSystemModel fsm, bool includeLastModified)
        {
            this.fsm = fsm;
            this.includeLastModified = includeLastModified;

            InsertRoots();
        }

        public FileSystemTreeModel(FileSystemModel fsm) : this(fsm, false)
        {
        }

        public void SetSorter(IComparer<object> sorter)
        {
            this.sorter = sorter;
        }

        public void InsertRoots()
        {
            RemoveAllChildren();

            foreach (object root in fsm.ListRoots())
            {
                this.InsertChildAt(new FolderNode(this, fsm, root), this.Children);
            }
        }

        public FolderNode NodeForFolder(Object obj)
        {
            Object parent = fsm.Parent(obj);
            TreeTableNode parentNode;

            if (parent == null)
            {
                parentNode = this;
            }
            else
            {
                parentNode = NodeForFolder(parent);
            }

            if (parentNode != null)
            {
                for (int i = 0; i < parentNode.Children; i++)
                {
                    FolderNode node = (FolderNode)parentNode.ChildAt(i);
                    if (fsm.Equals(node.Folder, obj))
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        public override string ColumnHeaderTextFor(int column)
        {
            switch (column)
            {
                case 0:
                    return "Folder";
                case 1:
                    return "Last modified";
                default:
                    return "";
            }
        }

        public FileSystemModel FileSystemModel
        {
            get
            {
                return fsm;
            }
        }
    }

    public class FolderNode : TreeTableNode
    {
        static FolderNode[] NO_CHILDREN = new FolderNode[0];

        private TreeTableNode parent;
        private FileSystemModel fsm;
        Object folder;
        FolderNode[] children;

        internal FolderNode(TreeTableNode parent, FileSystemModel fsm, object folder)
        {
            this.parent = parent;
            this.fsm = fsm;
            this.folder = folder;
        }

        public object Folder
        {
            get
            {
                return folder;
            }
        }

        public object DataAtColumn(int column)
        {
            switch (column)
            {
                case 0:
                    return fsm.NameOf(folder);
                case 1:
                    return LastModified;
                default:
                    return null;
            }
        }

        public object TooltipContentAtColumn(int column)
        {
            StringBuilder sb = new StringBuilder(fsm.PathOf(folder));
            DateTime lastModified = LastModified;
            if (lastModified != null)
            {
                sb.Append("\nLast modified: ").Append(lastModified);
            }
            return sb.ToString();
        }

        public TreeTableNode ChildAt(int idx)
        {
            return children[idx];
        }

        public int ChildIndexOf(TreeTableNode child)
        {
            for (int i = 0, n = children.Length; i < n; i++)
            {
                if (children[i] == child)
                {
                    return i;
                }
            }

            return -1;
        }

        public int Children
        {
            get
            {
                if (children == null)
                {
                    CollectChilds();
                }

                return children.Length;
            }
        }

        public TreeTableNode Parent
        {
            get
            {
                return parent;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return false;
            }
        }

        public FileSystemTreeModel TreeModel
        {
            get
            {
                TreeTableNode node = this.parent;
                TreeTableNode nodeParent;

                while ((nodeParent = node.Parent) != null)
                {
                    node = nodeParent;
                }

                return (FileSystemTreeModel) node;
            }
        }

        private void CollectChilds()
        {
            children = NO_CHILDREN;

            try
            {
                Object[] subFolder = fsm.ListFolder(folder, FolderFilter.Instance);

                if (subFolder != null && subFolder.Length > 0)
                {
                    IComparer<object> sorter = TreeModel.sorter;

                    if (sorter != null)
                    {
                        Array.Sort(subFolder, sorter);
                    }

                    FolderNode[] newChildren = new FolderNode[subFolder.Length];
                    for (int i = 0; i < subFolder.Length; i++)
                    {
                        newChildren[i] = new FolderNode(this, fsm, subFolder[i]);
                    }

                    children = newChildren;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private DateTime LastModified
        {
            get
            {
                if (parent is FileSystemTreeModel)
                {
                    // don't call getLastModified on roots - causes bad performance
                    // on windows when a DVD/CD/Floppy has no media inside
                    return DateTime.MinValue;
                }

                return new DateTime(fsm.LastModifiedOf(folder));
            }
        }
    }

    public class FolderFilter : FileFilter
    {
        public static FolderFilter Instance = new FolderFilter();

        public bool Accept(Object file)
        {
            return ((FileSystemObject)file).IsFolder;
        }
    }
}
