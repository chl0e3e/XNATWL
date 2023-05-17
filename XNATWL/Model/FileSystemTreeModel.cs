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
    /// <summary>
    /// A tree model which displays the folders of a FileSystemModel
    /// </summary>
    public class FileSystemTreeModel : AbstractTreeTableModel
    {
        private FileSystemModel _fileSystemModel;
        private bool _includeLastModified;

        internal IComparer<object> _sorter;

        public override int Columns
        {
            get
            {
                return _includeLastModified ? 2 : 1;
            }
        }

        public override event EventHandler<ColumnsChangedEventArgs> ColumnInserted;
        public override event EventHandler<ColumnsChangedEventArgs> ColumnDeleted;
        public override event EventHandler<ColumnHeaderChangedEventArgs> ColumnHeaderChanged;

        public FileSystemTreeModel(FileSystemModel fsm, bool includeLastModified)
        {
            this._fileSystemModel = fsm;
            this._includeLastModified = includeLastModified;

            InsertRoots();
        }

        public FileSystemTreeModel(FileSystemModel fsm) : this(fsm, false)
        {
        }

        /// <summary>
        /// Sets the sorter used for sorting folders (the root nodes are not sorted).<br/><br/>
        /// Will call insertRoots() when the sorter is changed.
        /// </summary>
        /// <param name="sorter">The new sorter - can be null</param>
        public void SetSorter(IComparer<object> sorter)
        {
            this._sorter = sorter;
        }

        /// <summary>
        /// Removes all nodes from the tree and creates the root nodes
        /// </summary>
        public void InsertRoots()
        {
            RemoveAllChildren();

            foreach (object root in _fileSystemModel.ListRoots())
            {
                this.InsertChildAt(new FolderNode(this, _fileSystemModel, root), this.Children);
            }
        }

        public FolderNode NodeForFolder(Object obj)
        {
            Object parent = _fileSystemModel.Parent(obj);
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
                    if (_fileSystemModel.Equals(node.Folder, obj))
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
                return _fileSystemModel;
            }
        }
    }

    public class FolderNode : TreeTableNode
    {
        static FolderNode[] NO_CHILDREN = new FolderNode[0];

        private TreeTableNode _parent;
        private FileSystemModel _fileSystemModel;
        Object _folder;
        FolderNode[] _children;

        internal FolderNode(TreeTableNode parent, FileSystemModel fsm, object folder)
        {
            this._parent = parent;
            this._fileSystemModel = fsm;
            this._folder = folder;
        }

        public object Folder
        {
            get
            {
                return _folder;
            }
        }

        public object DataAtColumn(int column)
        {
            switch (column)
            {
                case 0:
                    return _fileSystemModel.NameOf(_folder);
                case 1:
                    return LastModified;
                default:
                    return null;
            }
        }

        public object TooltipContentAtColumn(int column)
        {
            StringBuilder sb = new StringBuilder(_fileSystemModel.PathOf(_folder));
            DateTime lastModified = LastModified;
            if (lastModified != null)
            {
                sb.Append("\nLast modified: ").Append(lastModified);
            }
            return sb.ToString();
        }

        public TreeTableNode ChildAt(int idx)
        {
            return _children[idx];
        }

        public int ChildIndexOf(TreeTableNode child)
        {
            for (int i = 0, n = _children.Length; i < n; i++)
            {
                if (_children[i] == child)
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
                if (_children == null)
                {
                    CollectChilds();
                }

                return _children.Length;
            }
        }

        public TreeTableNode Parent
        {
            get
            {
                return _parent;
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
                TreeTableNode node = this._parent;
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
            _children = NO_CHILDREN;

            try
            {
                Object[] subFolder = _fileSystemModel.ListFolder(_folder, FolderFilter.Instance);

                if (subFolder != null && subFolder.Length > 0)
                {
                    IComparer<object> sorter = TreeModel._sorter;

                    if (sorter != null)
                    {
                        Array.Sort(subFolder, sorter);
                    }

                    FolderNode[] newChildren = new FolderNode[subFolder.Length];
                    for (int i = 0; i < subFolder.Length; i++)
                    {
                        newChildren[i] = new FolderNode(this, _fileSystemModel, subFolder[i]);
                    }

                    _children = newChildren;
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
                if (_parent is FileSystemTreeModel)
                {
                    // don't call getLastModified on roots - causes bad performance
                    // on windows when a DVD/CD/Floppy has no media inside
                    return DateTime.MinValue;
                }

                return new DateTime(_fileSystemModel.LastModifiedOf(_folder));
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
