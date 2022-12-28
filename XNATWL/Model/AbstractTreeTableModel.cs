namespace XNATWL.Model
{
    public abstract class AbstractTreeTableModel : AbstractTableColumnHeaderModel, TreeTableModel
    {
        private List<TreeTableNode> _children;
    }
}