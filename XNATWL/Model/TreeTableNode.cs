using System;

namespace XNATWL.Model
{
    public interface TreeTableNode
    {
        object DataAtColumn(int column);

        object TooltipContentAtColumn(int column);

        TreeTableNode Parent
        {
            get;
        }

        bool IsLeaf
        {
            get;
        }

        int Children
        {
            get;
        }

        TreeTableNode ChildAt(int idx);

        int ChildIndexOf(TreeTableNode child);
    }
}