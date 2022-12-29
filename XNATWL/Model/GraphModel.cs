﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface GraphModel
    {
        int Lines
        {
            get;
        }

        GraphLineModel LineAt(int index);

        bool ScaleLinesIndependent();
    }
}