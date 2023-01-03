using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;

namespace XNATWL
{
    public class Table : TableBase
    {
        TableModel model;

        public Table()
        {
        }

        public Table(TableModel model) : this()
        {
            setModel(model);
        }

        public TableModel getModel()
        {
            return model;
        }

        public void setModel(TableModel model)
        {
            if (this.model != null)
            {
                this.model.RowsDeleted -= Model_RowsDeleted;
                this.model.AllChanged -= Model_AllChanged;
                this.model.CellChanged -= Model_CellChanged;
                this.model.RowsChanged -= Model_RowsChanged;
                this.model.RowsInserted -= Model_RowsInserted;
                this.model.ColumnHeaderChanged -= Model_ColumnHeaderChanged;
                this.model.ColumnInserted -= Model_ColumnInserted;
                this.model.ColumnDeleted -= Model_ColumnDeleted;
            }
            this.columnHeaderModel = model;
            this.model = model;
            if (this.model != null)
            {
                numRows = model.Rows;
                numColumns = model.Columns;
                this.model.RowsDeleted += Model_RowsDeleted;
                this.model.AllChanged += Model_AllChanged;
                this.model.CellChanged += Model_CellChanged;
                this.model.RowsChanged += Model_RowsChanged;
                this.model.RowsInserted += Model_RowsInserted;
                this.model.ColumnHeaderChanged += Model_ColumnHeaderChanged;
                this.model.ColumnInserted += Model_ColumnInserted;
                this.model.ColumnDeleted += Model_ColumnDeleted;
            }
            else
            {
                numRows = 0;
                numColumns = 0;
            }
            modelAllChanged();
        }

        private void Model_ColumnDeleted(object sender, ColumnsChangedEventArgs e)
        {
            checkColumnRange(e.Index, e.Count);
            numColumns = model.Columns;
            modelColumnsDeleted(e.Count, e.Count);
        }

        private void Model_ColumnInserted(object sender, ColumnsChangedEventArgs e)
        {
            numColumns = model.Columns;
            modelColumnsInserted(e.Count, e.Count);
        }

        private void Model_ColumnHeaderChanged(object sender, ColumnHeaderChangedEventArgs e)
        {
            modelColumnHeaderChanged(e.Column);
        }

        private void Model_RowsInserted(object sender, TableRowModificationEventArgs e)
        {
            numRows = model.Rows;
            modelRowsInserted(e.Index, e.Count);
        }

        private void Model_RowsChanged(object sender, TableRowModificationEventArgs e)
        {
            modelRowsChanged(e.Index, e.Count);
        }

        private void Model_CellChanged(object sender, TableCellModificationEventArgs e)
        {
            modelCellChanged(e.Row, e.Column);
        }

        private void Model_AllChanged(object sender, TableAllChangedEventArgs e)
        {
            numRows = model.Rows;
            numColumns = model.Columns;
            modelAllChanged();
        }

        private void Model_RowsDeleted(object sender, TableRowModificationEventArgs e)
        {
            checkRowRange(e.Index, e.Count);
            numRows = model.Rows;
            modelRowsDeleted(e.Index, e.Count);
        }

        public override Object getCellData(int row, int column, TreeTableNode node)
        {
            return model.CellAt(row, column);
        }

        public override TreeTableNode getNodeFromRow(int row)
        {
            return null;
        }

        public override Object getTooltipContentFromRow(int row, int column)
        {
            return model.TooltipAt(row, column);
        }
    }
}
