using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;

namespace XNATWL
{
    public enum TableSelectionGranularity
    {
        ROWS,
        CELLS
    }

    public interface TableSelectionManager
    {
        TableSelectionModel getSelectionModel();

        void setAssociatedTable(TableBase tableBase);

        TableSelectionGranularity getSelectionGranularity();

        bool handleKeyStrokeAction(String action, Event evt);

        bool handleMouseEvent(int row, int column, Event evt);

        bool isRowSelected(int row);

        bool isCellSelected(int row, int column);

        int getLeadRow();

        int getLeadColumn();

        void modelChanged();

        void rowsInserted(int index, int count);

        void rowsDeleted(int index, int count);

        void columnInserted(int index, int count);

        void columnsDeleted(int index, int count);
    }

}
