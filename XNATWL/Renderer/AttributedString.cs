using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL.Renderer
{
    public interface AttributedString : AnimationState, CharSequence
    {
        int Position
        {
            get;
            set;
        }

        int Advance();
    }
}
