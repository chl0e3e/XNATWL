using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class FileSystemTreeModel : AbstractTreeTableModel
    {
        private FileSystemModel fsm;
        private bool includeLastModified;

        internal Comparer<object> sorter;

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
                    Comparer<object> sorter = TreeModel.sorter;

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

        public bool Accept(FileSystemModel model, Object file)
        {
            return model.IsFolder(file);
        }
    }
}
