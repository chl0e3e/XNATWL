using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public enum SortOrder
    {
        ASCENDING,
        DESCENDING
    }

    public class SortOrderStatics
    {
        public static SortOrder SortOrder_Invert(SortOrder order)
        {
            if (order == SortOrder.ASCENDING)
            {
                return SortOrder.DESCENDING;
            }

            return SortOrder.ASCENDING;
        }
    }
}
