using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.TextArea
{
    public interface StyleSheetResolver
    {
        void StartLayout();

        Style Resolve(Style style);

        void LayoutFinished();
    }
}
