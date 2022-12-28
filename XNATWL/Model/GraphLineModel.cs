using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface GraphLineModel
    {
        string VisualStyleName
        {
            get;
        }

        int Points
        {
            get;
        }

        float Point(int index);

        float MinValue
        {
            get;
        }

        float MaxValue
        {
            get;
        }
    }
}
