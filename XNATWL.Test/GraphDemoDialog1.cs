using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;

namespace XNATWL.Test
{
    public class GraphDemoDialog1 : FadeFrame
    {
        private SimpleGraphLineModel _gmMsPerFrame;
        private long _lastTime = NanoTime();

        private static long NanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

        public GraphDemoDialog1()
        {
            _gmMsPerFrame = new SimpleGraphLineModel("default", 100, 0, 30);
            GraphLineModel[] lineModel = new GraphLineModel[] { _gmMsPerFrame };

            Graph graph = new Graph(new SimpleGraphModel(lineModel));
            graph.SetTheme("/graph");

            SetTheme(SimpleTest.WITH_TITLE);
            SetTitle("MS per frame");
            Add(graph);
        }

        protected override void Paint(GUI gui)
        {
            long time = NanoTime();
            _gmMsPerFrame.AddPoint((float)(time - _lastTime) * 1e-6f);
            _lastTime = time;

            base.Paint(gui);
        }
    }
}
