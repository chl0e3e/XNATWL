using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public interface CharSequence
    {
        char CharAt(int index);

        string SubSequence(int start, int end);

        string Value
        {
            get;
        }

        int Length
        {
            get;
        }
    }
}
