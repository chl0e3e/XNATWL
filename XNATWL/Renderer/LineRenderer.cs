﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface LineRenderer
    {
        void DrawLine(float[] pts, int numPts, float width, Color color, bool drawAsLoop);
    }
}
